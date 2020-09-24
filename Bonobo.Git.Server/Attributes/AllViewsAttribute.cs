using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Unity;

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
        [Dependency]
        public IRepositoryPermissionService RepoPermissions { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.Controller.ViewBag.PermittedRepositories = PopulateRepoGoToList(filterContext.HttpContext.User.Id(), filterContext.Controller.ControllerContext);
        }

        private List<SelectListItem> PopulateRepoGoToList(Guid id, ControllerContext ControllerContext)
        {
            var pullList = RepoPermissions.GetAllPermittedRepositories(id, RepositoryAccessLevel.Pull);
            var adminList = RepoPermissions.GetAllPermittedRepositories(id, RepositoryAccessLevel.Administer);
            var firstList = pullList.Union(adminList, new InlineComparer<RepositoryModel>((lhs, rhs) => lhs.Id == rhs.Id, obj => obj.Id.GetHashCode()))
                    .OrderBy(x => x.Name.ToLowerInvariant())
                    .GroupBy(x => x.Group == null ? Resources.Repository_No_Group : x.Group);
            List<SelectListItem> items = new List<SelectListItem>();
            var u = new UrlHelper(ControllerContext.RequestContext);
            var groups = new Dictionary<string, SelectListGroup>();
            foreach (var grouped in firstList)
            {
                SelectListGroup group = null;
                string key = grouped.Key;
                if (!groups.TryGetValue(key, out group))
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