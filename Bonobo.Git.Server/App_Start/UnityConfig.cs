using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Git;
using Bonobo.Git.Server.Git.GitService;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook.Durability;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook.Hooks;
using Bonobo.Git.Server.Security;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Resolution;

namespace Bonobo.Git.Server
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            /* 
                The UnityDecoratorContainerExtension breaks resolving named type registrations, like:

                container.RegisterType<IMembershipService, ADMembershipService>("ActiveDirectory");
                container.RegisterType<IMembershipService, EFMembershipService>("Internal");
                IMembershipService membershipService = container.Resolve<IMembershipService>(AuthenticationSettings.MembershipService);

                Until this issue is resolved, the following two switch hacks will have to do
            */

            switch (AuthenticationSettings.MembershipService.ToLowerInvariant())
            {
                case "activedirectory":
                    container.RegisterType<IMembershipService, ADMembershipService>();
                    container.RegisterType<IRoleProvider, ADRoleProvider>();
                    container.RegisterType<ITeamRepository, ADTeamRepository>();
                    container.RegisterType<IRepositoryRepository, ADRepositoryRepository>();
                    container.RegisterType<IRepositoryPermissionService, RepositoryPermissionService>();
                    break;
                case "internal":
                    container.RegisterType<IMembershipService, EFMembershipService>();
                    container.RegisterType<IRoleProvider, EFRoleProvider>();
                    container.RegisterType<ITeamRepository, EFTeamRepository>();
                    container.RegisterType<IRepositoryRepository, EFRepositoryRepository>();
                    container.RegisterType<IRepositoryPermissionService, RepositoryPermissionService>();
                    break;
                default:
                    throw new ArgumentException("Missing declaration in web.config", "MembershipService");
            }

            switch (AuthenticationSettings.AuthenticationProvider.ToLowerInvariant())
            {
                case "windows":
                    container.RegisterType<IAuthenticationProvider, WindowsAuthenticationProvider>();
                    break;
                case "cookies":
                    container.RegisterType<IAuthenticationProvider, CookieAuthenticationProvider>();
                    break;
                case "federation":
                    container.RegisterType<IAuthenticationProvider, FederationAuthenticationProvider>();
                    break;
                default:
                    throw new ArgumentException("Missing declaration in web.config", "AuthenticationProvider");
            }

            container.RegisterFactory<IGitRepositoryLocator>((ctr, type, name) => new ConfigurationBasedRepositoryLocator(UserConfiguration.Current.Repositories));

            container.RegisterInstance(
                new GitServiceExecutorParams()
                {
                    GitPath = GetRootPath(ConfigurationManager.AppSettings["GitPath"]),
                    GitHomePath = GetRootPath(ConfigurationManager.AppSettings["GitHomePath"]),
                    RepositoriesDirPath = UserConfiguration.Current.Repositories,
                });

            container.RegisterType<IDatabaseResetManager, DatabaseResetManager>();

            if (AppSettings.IsPushAuditEnabled)
            {
                EnablePushAuditAnalysis(container);
            }

            container.RegisterType<IGitService, GitServiceExecutor>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        public static void RegisterFilters()
        {
            var oldProvider = FilterProviders.Providers.Single(f => f is FilterAttributeFilterProvider);
            FilterProviders.Providers.Remove(oldProvider);

            var provider = new UnityFilterAttributeFilterProvider(Container);
            FilterProviders.Providers.Add(provider);
        }

        private static void EnablePushAuditAnalysis(IUnityContainer container)
        {
            var isReceivePackRecoveryProcessEnabled = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["RecoveryDataPath"]);

            if (isReceivePackRecoveryProcessEnabled)
            {
                // git service execution durability registrations to enable receive-pack hook execution after failures
                container.RegisterType<IGitService, DurableGitServiceResult>();
                container.RegisterType<IHookReceivePack, ReceivePackRecovery>();
                container.RegisterType<IRecoveryFilePathBuilder, AutoCreateMissingRecoveryDirectories>();
                container.RegisterType<IRecoveryFilePathBuilder, OneFolderRecoveryFilePathBuilder>();
                container.RegisterInstance(new NamedArguments.FailedPackWaitTimeBeforeExecution(TimeSpan.FromSeconds(5 * 60)));

                container.RegisterInstance(new NamedArguments.ReceivePackRecoveryDirectory(
                    Path.IsPathRooted(ConfigurationManager.AppSettings["RecoveryDataPath"]) ?
                        ConfigurationManager.AppSettings["RecoveryDataPath"] :
                        HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["RecoveryDataPath"])));
            }

            // base git service executor
            container.RegisterType<IGitService, ReceivePackParser>();
            container.RegisterType<GitServiceResultParser, GitServiceResultParser>();

            // receive pack hooks
            container.RegisterType<IHookReceivePack, AuditPusherToGitNotes>();
            container.RegisterType<IHookReceivePack, NullReceivePackHook>();

            // run receive-pack recovery if possible
            if (isReceivePackRecoveryProcessEnabled)
            {
                var recoveryProcess = container.Resolve<ReceivePackRecovery>(
                    new ParameterOverride(
                        "failedPackWaitTimeBeforeExecution",
                        new NamedArguments.FailedPackWaitTimeBeforeExecution(TimeSpan.FromSeconds(0)))); // on start up set time to wait = 0 so that recovery for all waiting packs is attempted

                try
                {
                    recoveryProcess.RecoverAll();
                }
                catch
                {
                    // don't let a failed recovery attempt stop start-up process
                }
                finally
                {
                }
            }
        }

        private static string GetRootPath(string path)
        {
            return Path.IsPathRooted(path) ?
                path :
                HostingEnvironment.MapPath(path);
        }
    }
}