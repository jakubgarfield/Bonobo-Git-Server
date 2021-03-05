using System.Configuration;
using System.IO;
using System.Web.Hosting;

namespace Bonobo.Git.Server.Configuration
{
    internal class HostingEnvironmentPathResolver : IPathResolver
    {
        public string Resolve(string path) => Path.IsPathRooted(path) ? path : HostingEnvironment.MapPath(path);

        public string ResolveWithConfiguration(string configKey) => Resolve(ConfigurationManager.AppSettings[configKey]);
    }
}