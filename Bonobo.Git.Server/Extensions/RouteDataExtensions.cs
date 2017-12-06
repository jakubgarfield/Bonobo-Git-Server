using System;
using Microsoft.AspNetCore.Routing;

namespace Bonobo.Git.Server.Extensions
{
    public static class RouteDataExtensions
    {
        public static string GetRequiredString(this RouteData routeData, string valueName)
        {
            if (routeData == null)
                throw new ArgumentNullException(nameof(routeData));

            object value;
            if (routeData.Values.TryGetValue(valueName, out value))
            {
                string valueString = value as string;
                if (!String.IsNullOrEmpty(valueString))
                {
                    return valueString;
                }
            }
            throw new InvalidOperationException(string.Format("RouteData_RequiredValue ({0})", valueName));
        }
    }
}
