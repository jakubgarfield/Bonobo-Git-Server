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
using System.Web.Security;
using System.Security.Principal;
using Bonobo.Git.Server.DAL;

namespace Bonobo.Git.Server
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public const string GitRepositoryPrefix = "Git.aspx/";

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

            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
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

            BonoboGitServerContext.CreateDatabaseInNotExists();
        }

#if !DEBUG
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
#endif

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (Context.User == null)
            {
                var oldTicket = ExtractTicketFromCookie(Context, FormsAuthentication.FormsCookieName);
                if (oldTicket != null && !oldTicket.Expired)
                {
                    var ticket = oldTicket;
                    if (FormsAuthentication.SlidingExpiration)
                    {
                        ticket = FormsAuthentication.RenewTicketIfOld(oldTicket);
                        if (ticket == null)
                        {
                            return;
                        }
                    }

                    Context.User = new GenericPrincipal(new FormsIdentity(ticket), new string[0]);
                    if (ticket != oldTicket)
                    {
                        string cookieValue = FormsAuthentication.Encrypt(ticket);
                        var cookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName] ?? new HttpCookie(FormsAuthentication.FormsCookieName, cookieValue) { Path = ticket.CookiePath };
                        if (ticket.IsPersistent)
                        {
                            cookie.Expires = ticket.Expiration;
                        }
                        cookie.Value = cookieValue;
                        cookie.Secure = FormsAuthentication.RequireSSL;
                        cookie.HttpOnly = true;
                        if (FormsAuthentication.CookieDomain != null)
                        {
                            cookie.Domain = FormsAuthentication.CookieDomain;
                        }
                        Context.Response.Cookies.Remove(cookie.Name);
                        Context.Response.Cookies.Add(cookie);
                    }
                }
            }
        }

        private static FormsAuthenticationTicket ExtractTicketFromCookie(HttpContext context, string name)
        {
            FormsAuthenticationTicket ticket = null;
            string encryptedTicket = null;

            var cookie = context.Request.Cookies[name];
            if (cookie != null)
            {
                encryptedTicket = cookie.Value;
            }

            if (!string.IsNullOrEmpty(encryptedTicket))
            {
                try
                {
                    ticket = FormsAuthentication.Decrypt(encryptedTicket);
                }
                catch
                {
                    context.Request.Cookies.Remove(name);
                }

                if (ticket != null && !ticket.Expired)
                {
                    return ticket;
                }

                context.Request.Cookies.Remove(name);
            }

            return null;
        }
    }
}