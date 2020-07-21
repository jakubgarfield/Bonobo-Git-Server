using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Helpers
{
    public static class ConfigurationHelper
    {
        public static TimeSpan ParseTimeSpanOrDefault(string value, TimeSpan otherwise)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return otherwise;
            }

            TimeSpan res;

            if (TimeSpan.TryParse(value, out res))
            {
                return res;
            }

            return otherwise;
        }
    }
}