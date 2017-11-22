using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;

namespace Bonobo.Git.Server.Controllers
{
    public class TeamController : Controller
    {
        public IMembershipService MembershipService { get; set; }
        public IRepositoryRepository RepositoryRepository { get; set; }
        public ITeamRepository TeamRepository { get; set; }

        public TeamController(IMembershipService membershipService, IRepositoryRepository repositoryRepository, ITeamRepository teamRepository)
        {
            MembershipService = membershipService;
            RepositoryRepository = repositoryRepository;
            TeamRepository = teamRepository;
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index()
        {
            return View(ConvertTeamModels(TeamRepository.GetAllTeams()));
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Edit(Guid id)
        {
            var model = ConvertEditTeamModel(TeamRepository.GetTeam(id));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Edit(TeamEditModel model)
        {
            if (ModelState.IsValid)
            {
                TeamRepository.Update(ConvertTeamDetailModel(model));
                ViewBag.UpdateSuccess = true;
            }
            model = ConvertEditTeamModel(TeamRepository.GetTeam(model.Id));
            return View(model);
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Create()
        {
            var model = new TeamEditModel
            {
                AllUsers = MembershipService.GetAllUsers().ToArray(),
                SelectedUsers = new UserModel[] { }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Create(TeamEditModel model)
        {
            while (!String.IsNullOrEmpty(model.Name) && model.Name.Last() == ' ')
            {
                model.Name = model.Name.Substring(0, model.Name.Length - 1);
            }

            if (ModelState.IsValid)
            {
                var teammodel = ConvertTeamDetailModel(model);
                if (TeamRepository.Create(teammodel))
                {
                    TempData["CreateSuccess"] = true;
                    TempData["NewTeamId"] = teammodel.Id;
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", Resources.Team_Create_Failure);
                }
            }

            return View(model);
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(Guid id)
        {
            return View(ConvertEditTeamModel(TeamRepository.GetTeam(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(TeamEditModel model)
        {
            if (model != null && model.Id != null)
            {
                TeamModel team = TeamRepository.GetTeam(model.Id);
                TeamRepository.Delete(team.Id);
                TempData["DeleteSuccess"] = true;
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [WebAuthorize]
        public ActionResult Detail(Guid id)
        {
            return View(ConvertDetailTeamModel(TeamRepository.GetTeam(id)));
        }

        private TeamDetailModelList ConvertTeamModels(IEnumerable<TeamModel> models)
        {
            var result = new TeamDetailModelList();
            result.IsReadOnly = MembershipService.IsReadOnly();
            foreach (var item in models)
            {
                result.Add(ConvertDetailTeamModel(item));
            }
            return result;
        }

        private TeamEditModel ConvertEditTeamModel(TeamModel model)
        {
            return model == null ? null : new TeamEditModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                AllUsers = MembershipService.GetAllUsers().ToArray(),
                SelectedUsers = model.Members.ToArray(),
            };
        }

        private TeamDetailModel ConvertDetailTeamModel(TeamModel model)
        {
            return model == null ? null : new TeamDetailModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Members = model.Members.ToArray(),
                Repositories = RepositoryRepository.GetTeamRepositories(new[] { model.Id }).ToArray(),
                IsReadOnly = MembershipService.IsReadOnly()
            };
        }

        private TeamModel ConvertTeamDetailModel(TeamEditModel model)
        {
            return new TeamModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Members = model.PostedSelectedUsers == null ? new UserModel[0] : model.PostedSelectedUsers.Select(x => MembershipService.GetUserModel(x)).ToArray(),
            };
        }
    }
}
