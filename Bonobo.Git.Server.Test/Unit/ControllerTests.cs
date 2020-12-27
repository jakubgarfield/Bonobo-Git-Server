using Bonobo.Git.Server.Configuration;
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
    public partial class ControllerTests
    {
        private T SutAs<T>() where T : Controller => sut as T;

        private Mock<T> SetupMock<T>() where T : class
        {
            return new Mock<T>();
        }

        private void SetupUserAsAdmin()
        {
            claimsPrincipalMock.Setup(p => p.IsInRole(Definitions.Roles.Administrator))
                               .Returns(true);
        }


        private void SetHttpContextMockIntoSUT(Guid id)
        {
            var claimsIdentity = new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, id.ToString()) });

            IPrincipal user = CreateClaimsPrincipalFromClaimsIdentity(claimsIdentity);

            sut.ControllerContext = CreateControllerContextFromPrincipal(user);
        }

        private IPrincipal CreateClaimsPrincipalFromClaimsIdentity(ClaimsIdentity claimsIdentity)
        {
            // see: https://stackoverflow.com/a/1784417/41236
            claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.SetupGet(p => p.Identities)
                               .Returns(new List<ClaimsIdentity> { claimsIdentity });

            // see: https://stackoverflow.com/a/1783704/41236
            IPrincipal user = claimsPrincipalMock.Object;
            return user;
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

        private ControllerContext CreateControllerContext()
        {
            return CreateControllerContextFromPrincipal(new Mock<IPrincipal>().Object);
        }

        protected ControllerContext CreateControllerContextFromPrincipal(IPrincipal user)
        {
            httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.SetupGet(ctx => ctx.User).Returns(user);

            responseMock = new Mock<HttpResponseBase>();
            httpContextMock.SetupGet(c => c.Response)
                           .Returns(responseMock.Object);

            var controllerCtx = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };
            return controllerCtx;
        }

        private static ViewResult AssertAndGetViewResult(ActionResult result)
        {
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            return result as ViewResult;
        }

        private static RedirectToRouteResult AssertAndGetRedirectToRouteResult(ActionResult result)
        {
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));

            return result as RedirectToRouteResult;
        }

        private static void AssertRedirectToHomeUnauthorized(ActionResult result)
        {
            RedirectToRouteResult redirectToRouteResult = AssertAndGetRedirectToRouteResult(result);

            Assert.IsNotNull(redirectToRouteResult);
            Assert.AreEqual("Home", redirectToRouteResult.RouteValues["controller"]);
            Assert.AreEqual("Unauthorized", redirectToRouteResult.RouteValues["action"]);
        }

        private void SetupCookiesCollectionToHttpResponse()
        {
            HttpCookieCollection cookies = new HttpCookieCollection();
            responseMock.SetupGet(r => r.Cookies)
                        .Returns(cookies);
        }

        private static void ArrangeUserConfiguration()
        {
            Mock<IPathResolver> pathResolverMock = new Mock<IPathResolver>();
            pathResolverMock.Setup(p => p.Resolve(It.IsAny<string>()))
                            .Returns(".");
            pathResolverMock.Setup(p => p.ResolveWithConfiguration(It.IsAny<string>()))
                            .Returns("test.config");
            UserConfiguration.PathResolver = pathResolverMock.Object;
        }

        private static void ReinitializeStaticClass(Type type)
        {
            // This code allows reinitializing a static class 
            // see: https://stackoverflow.com/a/51758748/41236
            //
            type.TypeInitializer.Invoke(null, null);
        }

        private Controller sut;
        private Mock<ClaimsPrincipal> claimsPrincipalMock;
        private Mock<HttpContextBase> httpContextMock;
        private Mock<HttpResponseBase> responseMock;
    }
}
