using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using System;
using System.Web.Mvc;
using System.Web.UI;

namespace Bonobo.Git.Server.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class ValidationController : Controller
    {
        [Dependency]
        public IRepositoryRepository RepoRepo { get; set; }

        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public ITeamRepository TeamRepo { get; set; }

        public ActionResult UniqueNameRepo(string name, Guid? id)
        {
            bool isUnique = RepoRepo.NameIsUnique(name, id ?? Guid.Empty);
            return Json(isUnique, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UniqueNameUser(string Username, Guid? id)
        {
            var possibly_existent_user = MembershipService.GetUserModel(Username);
            bool exists = (possibly_existent_user != null) && (id != possibly_existent_user.Id);
            return Json(!exists, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UniqueNameTeam(string name, Guid? id)
        {
            var possibly_existing_team = TeamRepo.GetTeam(name);
            bool exists = (possibly_existing_team != null) && (id != possibly_existing_team.Id);
            // false when repo exists!
            return Json(!exists, JsonRequestBehavior.AllowGet);
        }
    }
}