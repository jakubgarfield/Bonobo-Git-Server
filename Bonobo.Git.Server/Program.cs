using System.Runtime.Caching;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Bonobo.Git.Server
{
    public class Program
    {
        public static ObjectCache Cache = MemoryCache.Default;

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
