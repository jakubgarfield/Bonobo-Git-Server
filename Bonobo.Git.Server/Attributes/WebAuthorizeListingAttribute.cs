using Bonobo.Git.Server.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server
{
    public class WebAuthorizeListingAttribute : WebAuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (UserConfiguration.Current.AllowAnonymousListing)
            {
                return;
            }

            base.OnAuthorization(filterContext);
        }
    }
}