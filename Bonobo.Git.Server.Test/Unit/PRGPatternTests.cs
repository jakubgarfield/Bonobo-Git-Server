using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
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
            principalMock.Setup(p => p.IsInRole(Definitions.Roles.Administrator))
                         .Returns(true);
        }

        private void SetHttpContextMockIntoSUT(Guid id)
        {
            var claimsIdentity = new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, id.ToString()) });

            // see: https://stackoverflow.com/a/1784417/41236
            principalMock = new Mock<ClaimsPrincipal>();
            principalMock.SetupGet(p => p.Identities)
                         .Returns(new List<ClaimsIdentity> { claimsIdentity });

            // see: https://stackoverflow.com/a/1783704/41236
            IPrincipal user = principalMock.Object;
            var httpCtxStub = new Mock<HttpContextBase>();
            httpCtxStub.SetupGet(ctx => ctx.User).Returns(user);

            var controllerCtx = new ControllerContext
            {
                HttpContext = httpCtxStub.Object
            };

            sut.ControllerContext = controllerCtx;
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

        private AccountController sut;
        private Mock<IMembershipService> membershipServiceMock;
        private Mock<IRoleProvider> roleProviderMock;
        private Mock<ClaimsPrincipal> principalMock;
    }
}
