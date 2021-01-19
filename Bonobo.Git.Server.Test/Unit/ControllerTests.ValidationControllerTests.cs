using Bonobo.Git.Server.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class ControllerTests
    {
        [TestClass]
        public class ValidationControllerTests : ControllerTests
        {
            [TestInitialize]
            public void TestInitialize()
            {
                sut = new ValidationController();
            }

            // Get UniqueNameRepo
            // Get UniqueNameUser
            // Get UniqueNameTeam
            // Get IsValidRegex
        }
    }
