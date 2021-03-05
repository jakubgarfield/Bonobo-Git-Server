using Bonobo.Git.Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.Unit
{
    [TestClass]
    public sealed class UserModelTest
    {
        [TestMethod]
        public void DisplayNameFormation()
        {
            Assert.AreEqual("John Smith", new UserModel { GivenName = "John", Surname = "Smith" }.DisplayName);
            Assert.AreEqual("John", new UserModel { GivenName = "John", Surname = null }.DisplayName);
            Assert.AreEqual("John", new UserModel { GivenName = "John", Surname = "" }.DisplayName);
            Assert.AreEqual("Smith", new UserModel { GivenName = null, Surname = "Smith" }.DisplayName);
            Assert.AreEqual("Smith", new UserModel { GivenName = "", Surname = "Smith" }.DisplayName);
            Assert.AreEqual("JohnSmith", new UserModel { Username="JohnSmith" }.DisplayName);
        }

        [TestMethod]
        public void SortNameFormation()
        {
            Assert.AreEqual("SmithJohn", new UserModel { GivenName = "John", Surname = "Smith" }.SortName);
            Assert.AreEqual("John", new UserModel { GivenName = "John", Surname = null }.SortName);
            Assert.AreEqual("John", new UserModel { GivenName = "John", Surname = "" }.SortName);
            Assert.AreEqual("Smith", new UserModel { GivenName = null, Surname = "Smith" }.SortName);
            Assert.AreEqual("Smith", new UserModel { GivenName = "", Surname = "Smith" }.SortName);
            Assert.AreEqual("JohnSmith", new UserModel { Username = "JohnSmith" }.SortName);
        }
    }
}