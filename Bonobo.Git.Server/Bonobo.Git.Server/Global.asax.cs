using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.ServiceModel.Activation;
using GitSharp;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using System.Globalization;
using System.Threading;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Controllers;
using System.Diagnostics;

namespace Bonobo.Git.Server
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public const string GitRepositoryPrefix = "Git.aspx/";
        public const string AnonymousGitRepositoryPrefix = "Git.aspx/Anonymous/";

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SecureInfoRefs", GitRepositoryPrefix + "{project}/info/refs",
                            new { controller = "Git", action = "SecureGetInfoRefs" },
                            new { method = new HttpMethodConstraint("GET") });

            routes.MapRoute("SecureUploadPack", GitRepositoryPrefix + "{project}/git-upload-pack",
                            new { controller = "Git", action = "SecureUploadPack" },
                            new { method = new HttpMethodConstraint("POST") });

            routes.MapRoute("SecureReceivePack", GitRepositoryPrefix + "{project}/git-receive-pack",
                            new { controller = "Git", action = "SecureReceivePack" },
                            new { method = new HttpMethodConstraint("POST") });

            routes.MapRoute("AnonymousInfoRefs", AnonymousGitRepositoryPrefix + "{project}/info/refs",
                            new { controller = "Git", action = "AnonymousGetInfoRefs" },
                            new { method = new HttpMethodConstraint("GET") });

            routes.MapRoute("AnonymousReceivePack", AnonymousGitRepositoryPrefix + "{project}/git-receive-pack",
                            new { controller = "Git", action = "AnonymousReceivePack" },
                            new { method = new HttpMethodConstraint("POST") });

            routes.MapRoute("AnonymousUploadPack", AnonymousGitRepositoryPrefix + "{project}/git-upload-pack",
                            new { controller = "Git", action = "AnonymousUploadPack" },
                            new { method = new HttpMethodConstraint("POST") });



            routes.MapRoute("IndexRoute", "{controller}/Index/",
                            new { action = "Index" });

            routes.MapRoute("CreateRoute", "{controller}/Create/",
                            new { action = "Create" });

            routes.MapRoute("RepositoryTree", "Repository/{id}/Tree/{name}/{*path}",
                            new { controller = "Repository", action = "Tree" });

            routes.MapRoute("RepositoryCommits", "Repository/{id}/Commits/{name}/",
                            new { controller = "Repository", action = "Commits" });

            routes.MapRoute("RepositoryCommit", "Repository/{id}/Commit/{commit}/",
                            new { controller = "Repository", action = "Commit" });

            routes.MapRoute("Repository", "Repository/{id}/{action}/",
                            new { controller = "Repository", action = "Detail" });

            routes.MapRoute("Account", "Account/{id}/{action}/",
                            new { controller = "Account", action = "Detail" });

            routes.MapRoute("Team", "Team/{id}/{action}/",
                            new { controller = "Team", action = "Detail" });

            routes.MapRoute("Default", "{controller}/{action}/{id}",
                            new { controller = "Home", action = "Index", id = "" });

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session != null)
            {
                var culture = (CultureInfo)this.Session["Culture"];
                if (culture == null)
                {
                    string langName = "en";

                    if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length != 0)
                    {
                        langName = HttpContext.Current.Request.UserLanguages[0].Substring(0, 2);
                    }
                    culture = new CultureInfo(langName);
                    this.Session["Culture"] = culture;
                }
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture.Name);
            }
        }

        private void RegisterDependencyResolver()
        {
            var container = new UnityContainer();

            container.RegisterType<IMembershipService, EFMembershipService>();
            container.RegisterType<IRepositoryPermissionService, EFRepositoryPermissionService>();
            container.RegisterType<IFormsAuthenticationService, FormsAuthenticationService>();
            container.RegisterType<ITeamRepository, EFTeamRepository>();
            container.RegisterType<IRepositoryRepository, EFRepositoryRepository>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            var oldProvider = FilterProviders.Providers.Single(f => f is FilterAttributeFilterProvider);
            FilterProviders.Providers.Remove(oldProvider);
            var provider = new UnityFilterAttributeFilterProvider(container);
            FilterProviders.Providers.Add(provider);
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);
            RegisterDependencyResolver();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            Response.Clear();
            HttpException httpException = exception as HttpException;

            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Home");
            if (httpException == null)
            {
                routeData.Values.Add("action", "Error");
                if (exception != null)
                {
                    Trace.TraceError("Error occured and caught in Global.asax - {0}", exception.ToString());
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
                        Trace.TraceError("Server Error occured and caught in Global.asax - {0}", exception.ToString());
                        break;
                    default:
                        routeData.Values.Add("action", "Error");
                        Trace.TraceError("Error occured and caught in Global.asax - {0}", exception.ToString());
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