using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using Bonobo.Git.Server.App_Start;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Git;
using Bonobo.Git.Server.Git.GitService;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using System.Runtime.Caching;
using Bonobo.Git.Server.Attributes;
using Microsoft.Practices.Unity.Mvc;
using System.Web.Configuration;
using System.Security.Claims;
using System.Web.Helpers;
using System.Web.Hosting;
using Bonobo.Git.Server.Application.Hooks;
using Serilog;

namespace Bonobo.Git.Server
{
    public class MvcApplication : HttpApplication
    {
        public static ObjectCache Cache = MemoryCache.Default; 

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session == null)
            {
                return;
            }

            var culture = (CultureInfo)Session["Culture"];
            if (culture == null)
            {
                culture = !String.IsNullOrEmpty(UserConfiguration.Current.DefaultLanguage)
                              ? new CultureInfo(UserConfiguration.Current.DefaultLanguage)
                              : null;

                if (culture == null)
                {
                    string langName = "en";

                    if (HttpContext.Current.Request.UserLanguages != null &&
                        HttpContext.Current.Request.UserLanguages.Length != 0 &&
                        HttpContext.Current.Request.UserLanguages[0].Length > 2)
                    {
                        langName = HttpContext.Current.Request.UserLanguages[0].Substring(0, 2);
                    }

                    culture = new CultureInfo(langName);
                    Session["Culture"] = culture;
                }
            }

            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture.Name);
        }

        protected void Application_Start()
        {
            ConfigureLogging();
            Log.Information("Bonobo starting");

            AreaRegistration.RegisterAllAreas();
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            UserConfiguration.Initialize();
            RegisterDependencyResolver();
            GlobalFilters.Filters.Add((AllViewsFilter)DependencyResolver.Current.GetService<AllViewsFilter>());

            var connectionstring = WebConfigurationManager.ConnectionStrings["BonoboGitServerContext"];
            if (connectionstring.ProviderName.ToLowerInvariant() == "system.data.sqlite")
            {
                if(!connectionstring.ConnectionString.ToLowerInvariant().Contains("binaryguid=false"))
                {
                    Log.Error("Please ensure that the sqlite connection string contains 'BinaryGUID=false;'.");
                    throw new ConfigurationErrorsException("Please ensure that the sqlite connection string contains 'BinaryGUID=false;'.");
                }
            }

            try
            {
                AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

                new AutomaticUpdater().Run();
                new RepositorySynchronizer().Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Startup exception");
                throw;
            }
        }

        private void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.RollingFile(GetLogFileNameFormat())
                .CreateLogger();
        }

        public static string GetLogFileNameFormat()
        {
            string logDirectory = ConfigurationManager.AppSettings["LogDirectory"];
            if (string.IsNullOrEmpty(logDirectory))
            {
                logDirectory = @"~\App_Data\Logs";
            }
            return Path.Combine(HostingEnvironment.MapPath(logDirectory), "log-{Date}.txt");
        }

        private static void RegisterDependencyResolver()
        {
            var container = new UnityContainer();

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

            container.RegisterType<IGitRepositoryLocator, ConfigurationBasedRepositoryLocator>(
                new InjectionFactory((ctr, type, name) => {
                    return new ConfigurationBasedRepositoryLocator(UserConfiguration.Current.Repositories);
                })
            );
 
            container.RegisterInstance(
                new GitServiceExecutorParams()
                {
                    GitPath = GetRootPath(ConfigurationManager.AppSettings["GitPath"]),
                    GitHomePath = GetRootPath(ConfigurationManager.AppSettings["GitHomePath"]),
                    RepositoriesDirPath = UserConfiguration.Current.Repositories,
                });

            container.RegisterType<IDatabaseResetManager, DatabaseResetManager>();

            container.RegisterType<IGitService, GitServiceExecutor>("Executor");
            container.RegisterType<IAfterGitPushHandler, AfterPushAuditHandler>("AuditHandler");

            container.RegisterType<IGitService, GitHandlerInvocationService>(
                new InjectionConstructor(
                    new ResolvedParameter<IGitService>("Executor"),
                    new ResolvedParameter<IAfterGitPushHandler>("AuditHandler"),
                    new ResolvedParameter<IGitRepositoryLocator>()));

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            var oldProvider = FilterProviders.Providers.Single(f => f is FilterAttributeFilterProvider);
            FilterProviders.Providers.Remove(oldProvider);
            
            var provider = new UnityFilterAttributeFilterProvider(container);
            FilterProviders.Providers.Add(provider);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            if (exception != null)
            {
                Response.Clear();
                HttpException httpException = exception as HttpException;

                RouteData routeData = new RouteData();
                routeData.Values.Add("controller", "Home");
                if (httpException == null)
                {
                    routeData.Values.Add("action", "Error");
                    if (exception != null)
                    {
                        Log.Error(exception, "Exception caught in Global.asax1");
                    }
                }
                else
                {
                    switch (httpException.GetHttpCode())
                    {
                        case 404:
                            routeData.Values.Add("action", "PageNotFound");
                            break;
                        case 500:
                            routeData.Values.Add("action", "ServerError");
                            Log.Error(exception, "500 Exception caught in Global.asax");
                            break;
                        default:
                            routeData.Values.Add("action", "Error");
                            Log.Error(exception, "Exception caught in Global.asax (code {Code})", httpException.GetHttpCode());
                            break;
                    }
                }
                Server.ClearError();
                Response.TrySkipIisCustomErrors = true;
                IController errorController = new HomeController();
                errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
            }
        }

        private static string GetRootPath(string path)
        {
            return Path.IsPathRooted(path) ?
                path :
                HttpContext.Current.Server.MapPath(path);
        }

    }
}
