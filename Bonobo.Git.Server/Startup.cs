using System;
using System.Diagnostics;
using Bonobo.Git.Server.Attributes;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Git;
using Bonobo.Git.Server.Git.GitService;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook.Durability;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook.Hooks;
using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Bonobo.Git.Server.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            /* 
                The UnityDecoratorContainerExtension breaks resolving named type registrations, like:

                container.RegisterType<IMembershipService, ADMembershipService>("ActiveDirectory");
                container.RegisterType<IMembershipService, EFMembershipService>("Internal");
                IMembershipService membershipService = container.Resolve<IMembershipService>(AuthenticationSettings.MembershipService);

                Until this issue is resolved, the following two switch hacks will have to do
            */

            var authSettings = Configuration.GetSection("AuthenticationSettings").Get<AuthenticationSettings>();
            var appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();

            switch (authSettings.MembershipService.ToLowerInvariant())
            {
                case "activedirectory":
                    services.AddTransient<IMembershipService, ADMembershipService>();
                    services.AddTransient<IRoleProvider, ADRoleProvider>();
                    services.AddTransient<ITeamRepository, ADTeamRepository>();
                    services.AddTransient<IRepositoryRepository, ADRepositoryRepository>();
                    services.AddTransient<IRepositoryPermissionService, RepositoryPermissionService>();
                    break;
                case "internal":
                    services.AddScoped<IMembershipService, EFMembershipService>();
                    services.AddScoped<IRoleProvider, EFRoleProvider>();
                    services.AddScoped<ITeamRepository, EFTeamRepository>();
                    services.AddScoped<IRepositoryRepository, EFRepositoryRepository>();
                    services.AddScoped<IRepositoryPermissionService, RepositoryPermissionService>();
                    break;
                default:
                    throw new ArgumentException("Missing declaration in web.config", "MembershipService");
            }

            switch (authSettings.AuthenticationProvider.ToLowerInvariant())
            {
                case "windows":
                    services.AddTransient<IAuthenticationProvider, WindowsAuthenticationProvider>();
                    WindowsAuthenticationProvider.Configure(services);
                    break;
                case "cookies":
                    services.AddTransient<IAuthenticationProvider, CookieAuthenticationProvider>();
                    CookieAuthenticationProvider.Configure(services);
                    break;
                case "federation":
                    services.AddTransient<IAuthenticationProvider, FederationAuthenticationProvider>();
                    FederationAuthenticationProvider.Configure(services, new FederationSettings());
                    break;
                default:
                    throw new ArgumentException("Missing declaration in web.config", "AuthenticationProvider");
            }

            services.AddTransient<IGitRepositoryLocator, ConfigurationBasedRepositoryLocator>(sc =>
                    new ConfigurationBasedRepositoryLocator(UserConfiguration.Current.Repositories));

            services.AddSingleton(
                new GitServiceExecutorParams()
                {
                    GitPath = /* GetRootPath*/(appSettings.GitPath),
                    GitHomePath = /*GetRootPath*/(appSettings.GitHomePath),
                    RepositoriesDirPath = UserConfiguration.Current.Repositories,
                });

            services.AddTransient<IDatabaseResetManager, DatabaseResetManager>();

            if (appSettings.IsPushAuditEnabled)
            {
                EnablePushAuditAnalysis(services, appSettings);
            }

            services.AddTransient<IGitService, GitServiceExecutor>();

            services.AddDbContext<BonoboGitServerContext>(options =>
            {
                //options.UseSqlite("Data Source=Bonobo.Git.Server.db");
                options.UseSqlServer("Server=.;Database=bonobogit;Integrated Security=True;");
            });

            services.Configure<AuthenticationSettings>(Configuration.GetSection("AuthenticationSettings"));
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(AllViewsFilter));
            });
        }

        private static void EnablePushAuditAnalysis(IServiceCollection services, AppSettings appSettings)
        {
            bool isReceivePackRecoveryProcessEnabled = !string.IsNullOrEmpty(appSettings.RecoveryDataPath);

            if (isReceivePackRecoveryProcessEnabled)
            {
                // git service execution durability registrations to enable receive-pack hook execution after failures
                services.AddTransient<IGitService, DurableGitServiceResult>();
                services.AddTransient<IHookReceivePack, ReceivePackRecovery>();
                services.AddTransient<IRecoveryFilePathBuilder, AutoCreateMissingRecoveryDirectories>();
                services.AddTransient<IRecoveryFilePathBuilder, OneFolderRecoveryFilePathBuilder>();
                services.AddSingleton(new NamedArguments.FailedPackWaitTimeBeforeExecution(TimeSpan.FromSeconds(5 * 60)));

                //services.AddSingleton(new NamedArguments.ReceivePackRecoveryDirectory(
                //    Path.IsPathRooted(appSettings.RecoveryDataPath) ?
                //        appSettings.RecoveryDataPath :
                //        HttpContext.Current.Server.MapPath(appSettings.RecoveryDataPath)));
            }

            // base git service executor
            services.AddTransient<IGitService, ReceivePackParser>();
            services.AddTransient<GitServiceResultParser, GitServiceResultParser>();

            // receive pack hooks
            services.AddTransient<IHookReceivePack, AuditPusherToGitNotes>();
            services.AddTransient<IHookReceivePack, NullReceivePackHook>();

            //// run receive-pack recovery if possible
            //if (isReceivePackRecoveryProcessEnabled)
            //{
            //    var recoveryProcess = container.Resolve<ReceivePackRecovery>(
            //        new ParameterOverride(
            //            "failedPackWaitTimeBeforeExecution",
            //            new NamedArguments.FailedPackWaitTimeBeforeExecution(TimeSpan.FromSeconds(0)))); // on start up set time to wait = 0 so that recovery for all waiting packs is attempted

            //    try
            //    {
            //        recoveryProcess.RecoverAll();
            //    }
            //    catch
            //    {
            //        // don't let a failed recovery attempt stop start-up process
            //    }
            //    finally
            //    {
            //        if (recoveryProcess != null)
            //        {
            //            container.Teardown(recoveryProcess);
            //        }
            //    }
            //}
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceScopeFactory scopeFactory, IOptions<AuthenticationSettings> authSettings)
        {
            ConfigureLogging();

            app.UseAuthentication();
            try
            {
                //AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
                using (var scope = scopeFactory.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetService<BonoboGitServerContext>();
                    new AutomaticUpdater().Run(scope.ServiceProvider, ctx, authSettings.Value);
                }
                using (var scope = scopeFactory.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetService<BonoboGitServerContext>();
                    var xxx = ctx.Model.ToDebugString();
                    var repositoryRepository = scope.ServiceProvider.GetService<IRepositoryRepository>();
                    new RepositorySynchronizer(repositoryRepository).Run();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("StartupException " + ex);
                Log.Error(ex, "Startup exception");
                throw;
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            var appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            if (appSettings.IsPushAuditEnabled)
            {
                bool isReceivePackRecoveryProcessEnabled = !string.IsNullOrEmpty(appSettings.RecoveryDataPath);
                if (isReceivePackRecoveryProcessEnabled)
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var recoveryProcess = scope.ServiceProvider.GetService<ReceivePackRecovery>();
                        try
                        {
                            recoveryProcess.RecoverAll();
                        }
                        catch
                        {
                            // don't let a failed recovery attempt stop start-up process
                        }
                    }

                }
            }
        }

        private void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                //.ReadFrom.AppSettings()
                //.WriteTo. //.RollingFile(Path.Combine(HostingEnvironment.MapPath(ConfigurationManager.AppSettings["LogDirectory"]), "log-{Date}.txt"))
                .CreateLogger();
        }
    }
}
