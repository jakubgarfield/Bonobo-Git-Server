using Bonobo.Git.Server.Attributes;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Bonobo.Git.Server.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class ValidationController : Controller
    {
        public IRepositoryRepository RepoRepo { get; set; }
        public IMembershipService MembershipService { get; set; }
        public ITeamRepository TeamRepo { get; set; }

        private readonly IActionContextAccessor actionContextAccessor;

        public ValidationController(IRepositoryRepository repoRepo, IMembershipService membershipService,
            ITeamRepository teamRepository, IActionContextAccessor actionContextAccessor)
        {
            RepoRepo = repoRepo;
            MembershipService = membershipService;
            TeamRepo = teamRepository;
            this.actionContextAccessor = actionContextAccessor;
        }

        public ActionResult UniqueNameRepo(string name, Guid? id)
        {
            bool isUnique = RepoRepo.NameIsUnique(name, id ?? Guid.Empty);
            return Json(isUnique);
        }

        public ActionResult UniqueNameUser(string Username, Guid? id)
        {
            var possibly_existent_user = MembershipService.GetUserModel(Username);
            bool exists = (possibly_existent_user != null) && (id != possibly_existent_user.Id);
            return Json(!exists);
        }

        public ActionResult UniqueNameTeam(string name, Guid? id)
        {
            var possibly_existing_team = TeamRepo.GetTeam(name);
            bool exists = (possibly_existing_team != null) && (id != possibly_existing_team.Id);
            // false when repo exists!
            return Json(!exists);
        }

        public ActionResult IsValidRegex(string LinksRegex)
        {
            var validationContext = new ValidationContext(actionContextAccessor.ActionContext);
            var isvalidregexattr = new IsValidRegexAttribute();
            var result = isvalidregexattr.GetValidationResult(LinksRegex, validationContext);
            if (result == System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                return Json(true);
            }
            return Json(result.ErrorMessage);
        }
    }
}