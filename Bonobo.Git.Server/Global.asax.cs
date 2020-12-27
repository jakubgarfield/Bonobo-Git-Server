using Bonobo.Git.Server.App_Start;
using Bonobo.Git.Server.Attributes;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data.Update;
using Serilog;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

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
            GlobalFilters.Filters.Add(DependencyResolver.Current.GetService<AllViewsFilter>());

            var connectionstring = WebConfigurationManager.ConnectionStrings["BonoboGitServerContext"];
            if (connectionstring.ProviderName.ToLowerInvariant() == "system.data.sqlite")
            {
                if (!connectionstring.ConnectionString.ToLowerInvariant().Contains("binaryguid=false"))
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
    }
}
