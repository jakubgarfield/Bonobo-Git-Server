using System;
using System.Diagnostics;
using System.Globalization;
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
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using Bonobo.Git.Server.Git.GitService;
using System.Configuration;
using System.IO;
using Bonobo.Git.Server.Git;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook.Hooks;

namespace Bonobo.Git.Server
{
    public class MvcApplication : HttpApplication
    {
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
                        HttpContext.Current.Request.UserLanguages.Length != 0)
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
            AreaRegistration.RegisterAllAreas();
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            RegisterDependencyResolver();

            new AutomaticUpdater().Run();
            UserConfiguration.Initialize();
            new RepositorySynchronizer().Run();
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (Context.User != null)
            {
                return;
            }

            var oldTicket = ExtractTicketFromCookie(Context, FormsAuthentication.FormsCookieName);
            if (oldTicket == null || oldTicket.Expired)
            {
                return;
            }

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
            if (ticket == oldTicket)
            {
                return;
            }

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

        private static void RegisterDependencyResolver()
        {
            var container = new UnityContainer()
                                .AddExtension(new UnityDecoratorContainerExtension());

            container.RegisterType<IMembershipService, EFMembershipService>();
            container.RegisterType<IRepositoryPermissionService, EFRepositoryPermissionService>();
            container.RegisterType<IFormsAuthenticationService, FormsAuthenticationService>();
            container.RegisterType<ITeamRepository, EFTeamRepository>();
            container.RegisterType<IRepositoryRepository, EFRepositoryRepository>();

            container.RegisterType<IGitRepositoryLocator, ConfigurationBasedRepositoryLocator>(new InjectionConstructor(UserConfiguration.Current.Repositories));
            container.RegisterInstance(new GitServiceExecutorParams()
            {
                GitPath = Path.IsPathRooted(ConfigurationManager.AppSettings["GitPath"])
                    ? ConfigurationManager.AppSettings["GitPath"]
                    : HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["GitPath"]),
                RepositoriesDirPath = UserConfiguration.Current.Repositories,
            });

            container.RegisterType<IGitService, ReceivePackParser>();
            container.RegisterType<IGitService, GitServiceExecutor>();

            // receive pack hooks
            container.RegisterType<IHookReceivePack, AuditPusherToGitNotes>();
            container.RegisterType<IHookReceivePack, NullReceivePackHook>();



            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            var oldProvider = FilterProviders.Providers.Single(f => f is FilterAttributeFilterProvider);
            FilterProviders.Providers.Remove(oldProvider);
            var provider = new UnityFilterAttributeFilterProvider(container);
            FilterProviders.Providers.Add(provider);
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

        private static FormsAuthenticationTicket ExtractTicketFromCookie(HttpContext context, string name)
        {
            FormsAuthenticationTicket ticket = null;
            string encryptedTicket = null;

            var cookie = context.Request.Cookies[name];
            if (cookie != null)
            {
                encryptedTicket = cookie.Value;
            }

            if (String.IsNullOrEmpty(encryptedTicket))
            {
                return null;
            }

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

            return null;
        }
    }
}
