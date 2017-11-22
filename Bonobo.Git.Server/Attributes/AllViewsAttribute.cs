using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Bonobo.Git.Server.Attributes
{
    public class InlineComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> getEquals;
        private readonly Func<T, int> getHashCode;

        public InlineComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
        {
            getEquals = equals;
            getHashCode = hashCode;
        }

        public bool Equals(T x, T y)
        {
            return getEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return getHashCode(obj);
        }
    }

    public class AllViewsFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Controller is Controller ctrl)
            {
                ctrl.ViewBag.PermittedRepositories = PopulateRepoGoToList(filterContext, filterContext.HttpContext.User.Id());
            }
        }

        private List<SelectListItem> PopulateRepoGoToList(ActionExecutingContext filterContext, Guid id)
        {
            var repoPermissions = filterContext.HttpContext.RequestServices.GetService<IRepositoryPermissionService>();

            var pullList = repoPermissions.GetAllPermittedRepositories(id, RepositoryAccessLevel.Pull);
            var adminList = repoPermissions.GetAllPermittedRepositories(id, RepositoryAccessLevel.Administer);
            var firstList = pullList.Union(adminList, new InlineComparer<RepositoryModel>((lhs, rhs) => lhs.Id == rhs.Id, obj => obj.Id.GetHashCode()))
                    .OrderBy(x => x.Name.ToLowerInvariant())
                    .GroupBy(x => x.Group ?? Resources.Repository_No_Group);

            List<SelectListItem> items = new List<SelectListItem>();
            var u = new UrlHelper(filterContext);
            var groups = new Dictionary<string, SelectListGroup>();
            foreach (var grouped in firstList)
            {
                string key = grouped.Key;
                if (!groups.TryGetValue(key, out SelectListGroup group))
                {
                    group = new SelectListGroup();
                    group.Name = key;
                    groups[key] = group;
                }
                foreach (var item in grouped)
                {
                    var slt = new SelectListItem
                    {
                        Text = item.Name,
                        Value = u.Action("Detail", "Repository", new { id = item.Id }),
                        Group = group,
                        /* This does not seem to work.
                         * If someone can figure out why we can remove the "Go to repository"
                         * from the drop down creation. */
                        Selected = item.Id == id,
                    };
                    items.Add(slt);
                }
            }
            return items;
        }

    }
}