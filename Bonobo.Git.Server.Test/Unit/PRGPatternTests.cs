using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Test.Unit
{
    [TestClass]
    public partial class PRGPatternTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            sut = new AccountController();
        }

        private void SetupUserAsAdmin()
        {
            claimsPrincipalMock.Setup(p => p.IsInRole(Definitions.Roles.Administrator))
                         .Returns(true);
        }

        private void SetHttpContextMockIntoSUT(Guid id)
        {
            var claimsIdentity = new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, id.ToString()) });

            // see: https://stackoverflow.com/a/1784417/41236
            claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.SetupGet(p => p.Identities)
                               .Returns(new List<ClaimsIdentity> { claimsIdentity });

            // see: https://stackoverflow.com/a/1783704/41236
            IPrincipal user = claimsPrincipalMock.Object;

            sut.ControllerContext = CreateControllerContextFromPrincipal(user);
        }

        private void SetupRolesProviderMockIntoSUT()
        {
            roleProviderMock = new Mock<IRoleProvider>();
            sut.RoleProvider = roleProviderMock.Object;
        }

        private void SetupMembershipServiceMockIntoSUT()
        {
            membershipServiceMock = new Mock<IMembershipService>();
            sut.MembershipService = membershipServiceMock.Object;
        }

        private void BindModelToController(object model)
        {
            // see: https://stackoverflow.com/a/5580363/41236
            var modelBinder = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ValueProvider = new NameValueCollectionValueProvider(new NameValueCollection(), CultureInfo.InvariantCulture)
            };
            var binder = new DefaultModelBinder().BindModel(new ControllerContext(), modelBinder);
            sut.ModelState.Clear();
            sut.ModelState.Merge(modelBinder.ModelState);
        }

        private ControllerContext CreateControllerContextFromPrincipal(IPrincipal user)
        {
            httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.SetupGet(ctx => ctx.User).Returns(user);

            var controllerCtx = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };
            return controllerCtx;
        }

        private static void AssertRedirectToHomeUnauthorized(ActionResult result)
        {
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));

            var redirectToRouteResult = result as RedirectToRouteResult;

            Assert.IsNotNull(redirectToRouteResult);
            Assert.AreEqual("Home", redirectToRouteResult.RouteValues["controller"]);
            Assert.AreEqual("Unauthorized", redirectToRouteResult.RouteValues["action"]);
        }

        private AccountController sut;
        private Mock<IMembershipService> membershipServiceMock;
        private Mock<IRoleProvider> roleProviderMock;
        private Mock<ClaimsPrincipal> claimsPrincipalMock;
        private Mock<HttpContextBase> httpContextMock;
    }
}
