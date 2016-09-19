using System;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpecsFor.Mvc;

namespace Bonobo.Git.Server.Test.IntegrationTests
{
    [TestClass]
    public abstract class IntegrationTestBase
    {
        protected static MvcWebApp app;
        protected static IntegrationTestHelpers ITH;
        protected static LoadedConfig lc;

        [ClassCleanup]
        public static void Cleanup()
        {
            app.Browser.Close();
        }

        [TestInitialize]
        public void InitTest()
        {
            // We can't use ClassInitialize in a base class
            if (app == null)
            {
                app = new MvcWebApp();
                lc = AssemblyStartup.LoadedConfig;
                ITH = new IntegrationTestHelpers(app, lc);
            }
            Console.WriteLine("TestInit");
            ITH.LoginAndResetDatabase();
        }

    }
}