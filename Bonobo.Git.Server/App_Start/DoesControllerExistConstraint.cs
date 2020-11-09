using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;

namespace Bonobo.Git.Server.App_Start
{
    /// <summary>
    /// Checks if controller and action is defined
    /// </summary>
    public class DoesControllerExistConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (routeDirection == RouteDirection.IncomingRequest)
            {
                var action = values["action"] as string;
                var controller = values["controller"] as string;

                return DoesControllerExist(controller, action);
            }
            else
                return true;
        }

        public static bool DoesControllerExist(string controller, string action = null)
        {
            if (controller is null)
                return false;

            var controllerFullName = string.Format("Bonobo.Git.Server.Controllers.{0}Controller", controller);

            var cont = Assembly.GetExecutingAssembly().GetType(controllerFullName);

            return cont != null && !string.IsNullOrEmpty(action) ? cont.GetMethod(action) != null : true;
        }
    }
}