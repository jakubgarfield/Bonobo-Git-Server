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
                ITH = new IntegrationTestHelpers(app);
            }
            Console.WriteLine("TestInit");
            ITH.LoginAndResetDatabase();
        }

    }
}