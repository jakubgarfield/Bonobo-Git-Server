using System.Configuration;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Bonobo.Git.Server
{
    public class WindowsActionFilterAttribute : ActionFilterAttribute
    {
        public static readonly bool IsWindowsModeActive =
            ((AuthenticationSection)ConfigurationManager.GetSection("system.web/authentication")).Mode ==
            AuthenticationMode.Windows;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (IsWindowsModeActive)
            {
                filterContext.Result = new RedirectResult("~/Home/Unauthorized");
            }
        }
    }
}