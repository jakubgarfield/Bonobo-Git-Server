using Bonobo.Git.Server.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server
{
    public class UserConfigurationRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (UserConfiguration.IsInitialized)
            {
                return;
            }

            filterContext.Result = new RedirectResult("~/Settings/Index");
        }
    }
}