using System;
using System.ComponentModel.DataAnnotations;
using Bonobo.Git.Server.Attributes;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;

namespace Bonobo.Git.Server.Controllers
{
    //[OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class ValidationController : Controller
    {
        public IRepositoryRepository RepoRepo { get; set; }
        public IMembershipService MembershipService { get; set; }
        public ITeamRepository TeamRepo { get; set; }

        public ValidationController(IRepositoryRepository repoRepo, IMembershipService membershipService, ITeamRepository teamRepo)
        {
            RepoRepo = repoRepo;
            MembershipService = membershipService;
            TeamRepo = teamRepo;
        }

        public ActionResult UniqueNameRepo(string name, Guid? id)
        {
            bool isUnique = RepoRepo.NameIsUnique(name, id ?? Guid.Empty);
            return Json(isUnique/*, JsonRequestBehavior.AllowGet*/);
        }

        public ActionResult UniqueNameUser(string Username, Guid? id)
        {
            var possibly_existent_user = MembershipService.GetUserModel(Username);
            bool exists = (possibly_existent_user != null) && (id != possibly_existent_user.Id);
            return Json(!exists/*, JsonRequestBehavior.AllowGet*/);
        }

        public ActionResult UniqueNameTeam(string name, Guid? id)
        {
            var possibly_existing_team = TeamRepo.GetTeam(name);
            bool exists = (possibly_existing_team != null) && (id != possibly_existing_team.Id);
            // false when repo exists!
            return Json(!exists/*, JsonRequestBehavior.AllowGet*/);
        }

        public ActionResult IsValidRegex(string LinksRegex)
        {
            var validationContext = new ValidationContext(Request.HttpContext);
            var isvalidregexattr = new IsValidRegexAttribute();
            var result = isvalidregexattr.GetValidationResult(LinksRegex, validationContext);
            if (result == System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                return Json(true/*, JsonRequestBehavior.AllowGet*/);
            }
            return Json(result.ErrorMessage/*, JsonRequestBehavior.AllowGet*/);
        }
    }
}