using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;

namespace Bonobo.Git.Server.Extensions
{
    public static class HostingEnvironmentExtensions
    {
        public static string MapPath(this IHostingEnvironment @this, string path)
        {
            if (path[0] == '~')
            {
                return Path.Combine(@this.ContentRootPath, path.Substring(1));
            }

            if (path[0] == '/')
            {
                return Path.Combine(@this.WebRootPath, path.Substring(1));
            }

            return Path.GetFullPath(path);
        }

        public static string MapPath(this HostingEnvironment @this, string path)
        {
            return ((IHostingEnvironment) @this).MapPath(path);
        }
    }
}
