using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Bonobo.Git.Server
{
    public class WindowsActionFilterAttribute : ActionFilterAttribute
    {
        public static readonly bool IsWindowsModeActive = new AuthenticationSection().Mode == AuthenticationMode.Windows;


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (IsWindowsModeActive)
            {
                filterContext.Result = new RedirectResult("~/Home/Unauthorized");
            }
        }
    }
}