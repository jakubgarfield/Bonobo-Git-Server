using System;
using System.IO;
using System.Collections.Generic;
using SpecsFor.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bonobo.Git.Server.App_GlobalResources;
using Newtonsoft.Json;

namespace Bonobo.Git.Server.Test.IntegrationTests
{
    [TestClass]
    public class AssemblyStartup
    {
        private static SpecsForIntegrationHost _host;
        public static string WebApplicationDirectory;

        private readonly static string ConfigFile = Path.GetFullPath(@"..\..\config.json");

        public static LoadedConfig LoadedConfig; 

        public static LoadedConfig LoadConfiguration()
        {
            var creds = new Dictionary<string, Dictionary<string, string>>
            {
                {"admin", new Dictionary<string, string>{{ "username", "admin" }, {"password", "admin"} } },
                {"user", new Dictionary<string, string>{{"username", "user"}, { "password", "uiae"} } }
            };
            if (File.Exists(ConfigFile))
            {
                creds = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(ConfigFile));
            }
            return new LoadedConfig(creds);
        }


#if !NCRUNCH
        [AssemblyInitialize()]
        static public void AssemblyInit(TestContext tc)
        {
            // so we can use the resources in our test even if your language is not en-US
            Resources.Culture = new System.Globalization.CultureInfo("en-US");

            LoadedConfig = LoadConfiguration();

            var config = new SpecsForMvcConfig();
            //SpecsFor.Mvc can spin up an instance of IIS Express to host your app 
            //while the specs are executing.  
            config.UseIISExpress()
                //To do that, it needs to know the name of the project to test...
                .With(Project.Named("Bonobo.Git.Server"))
                .UsePort(20000)
                //And optionally, it can apply Web.config transformations if you want 
                //it to.
                .ApplyWebConfigTransformForConfig("Test")
                .UseMSBuildExecutableAt(@"C:\Program Files (x86)\MSBuild\12.0\Bin\msbuild.exe");
                //.UseMSBuildExecutableAt(@"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe");

            //In order to leverage the strongly-typed helpers in SpecsFor.Mvc,
            //you need to tell it about your routes.  Here we are just calling
            //the infrastructure class from our MVC app that builds the RouteTable.
            config.BuildRoutesUsing(r => Bonobo.Git.Server.App_Start.RouteConfig.RegisterRoutes(r));
            //SpecsFor.Mvc can use either Internet Explorer or Firefox.  Support
            //for Chrome is planned for a future release.
            config.UseBrowser(BrowserDriver.InternetExplorer);

            // We cannot use the authenticate before each step helper
            // because it is only executed once per class.
            // So if class A test1 runs, then class B test2 (this leaves us logged out)
            // then class A test2 (which assumes we are logged in). It will fail
            //config.AuthenticateBeforeEachTestUsing<AuthHandler>();

            //Does your application send E-mails?  Well, SpecsFor.Mvc can intercept
            //those while your specifications are executing, enabling you to write
            //tests against the contents of sent messages.
            //config.InterceptEmailMessagesOnPort(13565);


            // If we set a WithPublishDirectory above, then this would change
            WebApplicationDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SpecsForMvc.TestSite");

            //The host takes our configuration and performs all the magic.  We
            //need to keep a reference to it so we can shut it down after all
            //the specifications have executed.
            _host = new SpecsForIntegrationHost(config);
    //try{
            _host.Start();
    //}
    //catch(Exception ex)
    //{
    //}

        }

        //The TearDown method will be called once all the specs have executed.
        //All we need to do is stop the integration host, and it will take
        //care of shutting down the browser, IIS Express, etc. 
        [AssemblyCleanup()]
        static public void TearDownTestRun()
        {
            _host.Shutdown();
        }
#endif // !NCRUNCH
    }
}
