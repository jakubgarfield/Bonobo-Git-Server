using System.Linq;
using System.Web.Mvc;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using Unity.Mvc5;

namespace Bonobo.Git.Server
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            container.RegisterType<IMembershipService, EFMembershipService>();
            container.RegisterType<IRepositoryPermissionService, EFRepositoryPermissionService>();
            container.RegisterType<IFormsAuthenticationService, FormsAuthenticationService>();
            container.RegisterType<ITeamRepository, EFTeamRepository>();
            container.RegisterType<IRepositoryRepository, EFRepositoryRepository>();
            
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            var oldProvider = FilterProviders.Providers.Single(f => f is FilterAttributeFilterProvider);
            FilterProviders.Providers.Remove(oldProvider);
            var provider = new UnityFilterAttributeFilterProvider(container);
            FilterProviders.Providers.Add(provider);
        }
    }
}
