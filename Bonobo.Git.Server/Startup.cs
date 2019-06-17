using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading;
using Bonobo.Git.Server.App_Start;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Extensions;
using Bonobo.Git.Server.Git;
using Bonobo.Git.Server.Git.GitService;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook.Durability;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook.Hooks;
using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace Bonobo.Git.Server
{
    public class Startup
    {
        private readonly IHostingEnvironment hostingEnvironment;

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureLogging(hostingEnvironment);
            Log.Information("Bonobo starting");

            UserConfiguration.Initialize(hostingEnvironment);
            //GlobalFilters.Filters.Add((AllViewsFilter)DependencyResolver.Current.GetService<AllViewsFilter>());

            var connectionstring = ConfigurationManager.ConnectionStrings["BonoboGitServerContext"];
            if (connectionstring.ProviderName.ToLowerInvariant() == "system.data.sqlite")
            {
                if (!connectionstring.ConnectionString.ToLowerInvariant().Contains("binaryguid=false"))
                {
                    Log.Error("Please ensure that the sqlite connection string contains 'BinaryGUID=false;'.");
                    throw new ConfigurationErrorsException("Please ensure that the sqlite connection string contains 'BinaryGUID=false;'.");
                }
            }

            services.AddDbContextPool<BonoboGitServerContext>(options =>
                options.UseSqlite(connectionstring.ConnectionString));

            services.AddMvc();

            services.AddHttpContextAccessor();

            services.AddAntiforgery();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Web", builder => builder.Requirements.Add(new WebRequirement()));
                options.AddPolicy("WebRepository", builder => builder.Requirements.Add(new WebRequirement()));
                options.AddPolicy("Git", builder => builder.Requirements.Add(new GitRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, WebAuthorizationHandler>();
            //services.AddSingleton<IAuthorizationHandler, WebRepositoryAuthorizationHandler>();
            //services.AddSingleton<IAuthorizationHandler, GitAuthorizationHandler>();

            switch (AuthenticationSettings.MembershipService.ToLowerInvariant())
            {
                case "activedirectory":
                    services.AddTransient<IMembershipService, ADMembershipService>();
                    services.AddTransient<IRoleProvider, ADRoleProvider>();
                    services.AddTransient<ITeamRepository, ADTeamRepository>();
                    services.AddTransient<IRepositoryRepository, ADRepositoryRepository>();
                    services.AddTransient<IRepositoryPermissionService, RepositoryPermissionService>();
                    break;
                case "internal":
                    var sp = services.BuildServiceProvider();
                    services.AddTransient<IMembershipService, EFMembershipService>(x => new EFMembershipService(() => sp.GetService<BonoboGitServerContext>()));
                    services.AddTransient<IRoleProvider, EFRoleProvider>(x => new EFRoleProvider(() => sp.GetService<BonoboGitServerContext>()));
                    services.AddTransient<ITeamRepository, EFTeamRepository>(x => new EFTeamRepository(() => sp.GetService<BonoboGitServerContext>()));
                    services.AddTransient<IRepositoryRepository, EFRepositoryRepository>(x => new EFRepositoryRepository(() => sp.GetService<BonoboGitServerContext>()));
                    services.AddTransient<IRepositoryPermissionService, RepositoryPermissionService>();
                    break;
                default:
                    throw new ArgumentException("Missing declaration in web.config", "MembershipService");
            }

            switch (AuthenticationSettings.AuthenticationProvider.ToLowerInvariant())
            {
                case "windows":
                    services.AddTransient<IAuthenticationProvider, WindowsAuthenticationProvider>();
                    break;
                case "cookies":
                    services.AddTransient<IAuthenticationProvider, CookieAuthenticationProvider>();
                    break;
                case "federation":
                    services.AddTransient<IAuthenticationProvider, FederationAuthenticationProvider>();
                    break;
                default:
                    throw new ArgumentException("Missing declaration in web.config", "AuthenticationProvider");
            }
            services.BuildServiceProvider().GetService<IAuthenticationProvider>().Configure(services);

            services.AddTransient<IGitRepositoryLocator, ConfigurationBasedRepositoryLocator>(serviceProvider =>
                new ConfigurationBasedRepositoryLocator(UserConfiguration.Current.Repositories)
            );

            services.AddSingleton(
                new GitServiceExecutorParams
                {
                    GitPath = GetRootPath(hostingEnvironment, ConfigurationManager.AppSettings["GitPath"]),
                    GitHomePath = GetRootPath(hostingEnvironment, ConfigurationManager.AppSettings["GitHomePath"]),
                    RepositoriesDirPath = UserConfiguration.Current.Repositories,
                });

            services.AddTransient<IDatabaseResetManager, DatabaseResetManager>();

            if (AppSettings.IsPushAuditEnabled)
            {
                EnablePushAuditAnalysis(services);
            }

            services.AddTransient<IGitService, GitServiceExecutor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(builder => RouteConfig.RegisterRoutes(builder));

            if (AppSettings.IsPushAuditEnabled)
            {
                var isReceivePackRecoveryProcessEnabled = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["RecoveryDataPath"]);
                // run receive-pack recovery if possible
                if (isReceivePackRecoveryProcessEnabled)
                {
                    var recoveryProcess = serviceProvider.GetService<ReceivePackRecovery>(); // on start up set time to wait = 0 so that recovery for all waiting packs is attempted

                    try
                    {
                        recoveryProcess.RecoverAll(TimeSpan.FromSeconds(0));
                    }
                    catch
                    {
                        // don't let a failed recovery attempt stop start-up process
                    }
                }
            }

            //app.Use(async (context, next) =>
            //{
            //    SetThreadCultureForRequest(context);
            //    await next.Invoke();
            //});

            try
            {
                //AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

                new AutomaticUpdater().Run(serviceProvider.GetService<BonoboGitServerContext>(), serviceProvider.GetService<IAuthenticationProvider>(), env);
                new RepositorySynchronizer(serviceProvider.GetService<IRepositoryRepository>()).Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Startup exception");
                throw;
            }
        }

        protected void SetThreadCultureForRequest(HttpContext context)
        {
            if (context.Session == null)
            {
                return;
            }

            var culture = (CultureInfo)JsonConvert.DeserializeObject(context.Session.GetString("Culture"));
            if (culture == null)
            {
                culture = !String.IsNullOrEmpty(UserConfiguration.Current.DefaultLanguage)
                              ? new CultureInfo(UserConfiguration.Current.DefaultLanguage)
                              : null;

                if (culture == null)
                {
                    string langName = "en";

                    var requestCultureFeature = context.Features.Get<IRequestCultureFeature>();

                    //if (context.Request.UserLanguages != null &&
                    //    context.Request.UserLanguages.Length != 0 &&
                    //    context.Request.UserLanguages[0].Length > 2)
                    //{
                    //    langName = context.Request.UserLanguages[0].Substring(0, 2);
                    //}

                    culture = new CultureInfo(langName);
                    context.Session.SetString("Culture", JsonConvert.SerializeObject(culture));
                }
            }

            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture.Name);
        }

        private void ConfigureLogging(IHostingEnvironment hostingEnvironment)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.RollingFile(GetLogFileNameFormat(hostingEnvironment))
                .CreateLogger();
        }

        public static string GetLogFileNameFormat(IHostingEnvironment hostingEnvironment)
        {
            string logDirectory = ConfigurationManager.AppSettings["LogDirectory"];
            if (string.IsNullOrEmpty(logDirectory))
            {
                logDirectory = @"~\App_Data\Logs";
            }
            return Path.Combine(hostingEnvironment.MapPath(logDirectory), "log-{Date}.txt");
        }

        private static void EnablePushAuditAnalysis(IServiceCollection services)
        {
            var isReceivePackRecoveryProcessEnabled = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["RecoveryDataPath"]);

            if (isReceivePackRecoveryProcessEnabled)
            {
                // git service execution durability registrations to enable receive-pack hook execution after failures
                services.AddTransient<IGitService, DurableGitServiceResult>();
                services.AddTransient<IHookReceivePack, ReceivePackRecovery>();
                services.AddTransient<IRecoveryFilePathBuilder, AutoCreateMissingRecoveryDirectories>();
                services.AddTransient<IRecoveryFilePathBuilder, OneFolderRecoveryFilePathBuilder>();

                //services.AddSingleton(new NamedArguments.ReceivePackRecoveryDirectory(
                //    Path.IsPathRooted(ConfigurationManager.AppSettings["RecoveryDataPath"]) ?
                //        ConfigurationManager.AppSettings["RecoveryDataPath"] :
                //        HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["RecoveryDataPath"])));
            }

            // base git service executor
            services.AddTransient<IGitService, ReceivePackParser>();
            services.AddTransient<GitServiceResultParser, GitServiceResultParser>();

            // receive pack hooks
            services.AddTransient<IHookReceivePack, AuditPusherToGitNotes>();
            services.AddTransient<IHookReceivePack, NullReceivePackHook>();
        }


        protected void ConfigureExceptionHandler(IApplicationBuilder app)
        {
            app.UseExceptionHandler(errorApp => { errorApp.Run(async context =>
            {
                var exceptionHandlerPathFeature =
                    context.Features.Get<IExceptionHandlerPathFeature>();
                Exception exception = exceptionHandlerPathFeature?.Error;
                if (exception != null)
                {
                    context.Response.Clear();

                    RouteData routeData = new RouteData();
                    routeData.Values.Add("controller", "Home");
                    switch (context.Response.StatusCode)
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
                            Log.Error(exception, "Exception caught in Startup.cs (code {Code})",
                                context.Response.StatusCode);
                            break;
                    }

                    // Server.ClearError();
                    // context.Response.TrySkipIisCustomErrors = true;
                    //Controller errorController = new HomeController();
                    //errorController.Execute(new RequestContext(new HttpContextWrapper(HostingApplication.Context),
                    //    routeData));
                }
            }); });
        }

        private static string GetRootPath(IHostingEnvironment hostingEnvironment, string path)
        {
            return Path.IsPathRooted(path) ?
                path :
                hostingEnvironment.MapPath(path);
        }
    }
}
