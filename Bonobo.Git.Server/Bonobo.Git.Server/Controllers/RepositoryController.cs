using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.App_GlobalResources;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Bonobo.Git.Server.Controllers
{
    public class RepositoryController : Controller
    {
        [Dependency]
        public ITeamRepository TeamRepository { get; set; }

        [Dependency]
        public IRepositoryRepository RepositoryRepository { get; set; }

        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }

        public ActionResult Index()
        {
            return View(GetIndexModel());
        }

        [RepositoryAuthorizeRedirect(RequiresRepositoryAdministrator = true)]
        public ActionResult Edit(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                var model = ConvertRepositoryModel(RepositoryRepository.GetRepository(id));
                PopulateEditData();
                return View(model);
            }
            return View();
        }

        [HttpPost]
        [RepositoryAuthorizeRedirect(RequiresRepositoryAdministrator = true)]
        public ActionResult Edit(RepositoryDetailModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Administrators.Contains(User.Identity.Name))
                {
                    RepositoryRepository.Update(ConvertRepositoryDetailModel(model));
                    ViewBag.UpdateSuccess = true;
                }
                else
                {
                    ModelState.AddModelError("Administrators", Resources.Repository_Edit_CantRemoveYourself);
                }
            }
            PopulateEditData();
            return View(model);
        }

        public ActionResult Create()
        {
            var model = new RepositoryDetailModel
            {
                Administrators = new string[] { User.Identity.Name },
            };
            PopulateEditData();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(RepositoryDetailModel model)
        {
            if (model != null && !String.IsNullOrEmpty(model.Name))
            {
                model.Name = Regex.Replace(model.Name, @"\s", "");     
            }

            if (String.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("Name", Resources.Repository_Create_NameFailure);
            }
            else if (ModelState.IsValid)
            {
                if (RepositoryRepository.Create(ConvertRepositoryDetailModel(model)))
                {
                    string path = Path.Combine(UserConfigurationManager.Repositories, model.Name);
                    if (!Directory.Exists(path))
                    {
                        using (var repository = new GitSharp.Core.Repository(new DirectoryInfo(path)))
                        {
                            repository.Create(true);
                        }
                        ViewBag.CreateSuccess = true;
                        return View("Index", GetIndexModel());
                    }
                    else
                    {
                        RepositoryRepository.Delete(model.Name);
                        ModelState.AddModelError("", Resources.Repository_Create_DirectoryExists);
                    }
                }
                else
                {
                    ModelState.AddModelError("", Resources.Repository_Create_Failure);
                }
            }
            PopulateEditData();
            return View(model);
        }

        [RepositoryAuthorizeRedirect(RequiresRepositoryAdministrator = true)]
        public ActionResult Delete(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                RepositoryRepository.Delete(id);
                string path = Path.Combine(UserConfigurationManager.Repositories, id);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                ViewBag.DeleteSuccess = true;
            }
            return View("Index", GetIndexModel());
        }

        [RepositoryAuthorizeRedirect]
        public ActionResult Detail(string id)
        {
            ViewBag.ID = id;
            if (!String.IsNullOrEmpty(id))
            {
                var model = ConvertRepositoryModel(RepositoryRepository.GetRepository(id));
                if (model != null)
                {
                    model.IsCurrentUserAdministrator = RepositoryPermissionService.IsRepositoryAdministrator(User.Identity.Name, id);
                }
                return View(model);
            }
            return View();
        }

        [RepositoryAuthorizeRedirect]
        public ActionResult Tree(string id, string name, string path)
        {
            ViewBag.ID = id;
            if (!String.IsNullOrEmpty(id))
            {
                path = path != null ? path.Replace(".browse", "") : null;
                var browser = new RepositoryBrowser(Path.Combine(UserConfigurationManager.Repositories, id));
                string branchName;
                var files = browser.Browse(name, path, out branchName);
                PopulateBranchesData(browser, branchName);
                PopulateAddressBarData(name, path);
                return DisplayFiles(files, path, id);
            }
            return View();
        }

        [RepositoryAuthorizeRedirect]
        public ActionResult Commits(string id, string name)
        {
            ViewBag.ID = id;
            if (!String.IsNullOrEmpty(id))
            {
                var browser = new RepositoryBrowser(Path.Combine(UserConfigurationManager.Repositories, id));
                string currentBranchName;
                var commits = browser.GetCommits(name, out currentBranchName);
                PopulateBranchesData(browser, currentBranchName);
                return View(new RepositoryCommitsModel { Commits = commits, Name = id });
            }

            return View();
        }

        [RepositoryAuthorizeRedirect]
        public ActionResult Commit(string id, string commit)
        {
            ViewBag.ID = id;
            if (!String.IsNullOrEmpty(id))
            {
                var browser = new RepositoryBrowser(Path.Combine(UserConfigurationManager.Repositories, id));
                var model = browser.GetCommitDetail(commit);
                model.Name = id;
                return View(model);
            }

            return View();
        }

        private ActionResult DisplayFiles(IEnumerable<RepositoryTreeDetailModel> files, string path, string id)
        {
            if (files != null)
            {
                var model = new RepositoryTreeModel();
                model.Name = id;
                model.IsTree = !(files.Count() == 1 && !files.First().IsTree && files.First().Path == path);
                if (model.IsTree)
                {
                    model.Files = files.OrderByDescending(i => i.IsTree).ThenBy(i => i.Name);
                }
                else
                {
                    model.File = files.First();
                    model.Text = FileDisplayHandler.GetText(model.File.Data);
                    model.IsTextFile = model.Text != null;

                    if (model.IsTextFile)
                    {
                        model.TextBrush = FileDisplayHandler.GetBrush(model.File.Name);
                    }

                    if (!model.IsTextFile)
                    {
                        model.IsImage = FileDisplayHandler.IsImage(model.File.Name);
                    }

                    if (!model.IsImage && !model.IsTextFile)
                    {
                        return File(new MemoryStream(model.File.Data), "application/octet-stream", model.File.Name);
                    }
                }

                return View(model);
            }

            return View();
        }


        private void PopulateAddressBarData(string name, string path)
        {
            ViewData["path"] = path;
            ViewData["name"] = name;
        }

        private void PopulateBranchesData(RepositoryBrowser browser, string branchName)
        {
            ViewData["currentBranch"] = branchName;
            ViewData["branches"] = browser.GetBranches();
        }

        private void PopulateEditData()
        {
            ViewData["AvailableUsers"] = MembershipService.GetAllUsers().Select(i => i.Username).ToArray();
            ViewData["AvailableAdministrators"] = ViewData["AvailableUsers"];
            ViewData["AvailableTeams"] = TeamRepository.GetAllTeams().Select(i => i.Name).ToArray();
        }

        private IEnumerable<RepositoryDetailModel> GetIndexModel()
        {
            if (User.IsInRole(Definitions.Roles.Administrator))
            {
                return ConvertRepositoryModels(RepositoryRepository.GetAllRepositories());
            }
            else
            {
                var userTeams = TeamRepository.GetTeams(User.Identity.Name).Select(i => i.Name).ToArray();
                var repositories = ConvertRepositoryModels(RepositoryRepository.GetPermittedRepositories(User.Identity.Name, userTeams));
                return repositories;
            }
        }

        private IList<RepositoryDetailModel> ConvertRepositoryModels(IList<RepositoryModel> models)
        {
            var result = new List<RepositoryDetailModel>();
            foreach (var item in models)
            {
                result.Add(ConvertRepositoryModel(item));
            }
            return result;
        }

        private RepositoryDetailModel ConvertRepositoryModel(RepositoryModel model)
        {
            return model == null ? null : new RepositoryDetailModel
            {
                Name = model.Name,
                Description = model.Description,
                Users = model.Users,
                Administrators = model.Administrators,
                Teams = model.Teams,
                IsCurrentUserAdministrator = model.Administrators.Contains(User.Identity.Name),
                AllowAnonymous = model.AnonymousAccess
            };
        }

        private RepositoryModel ConvertRepositoryDetailModel(RepositoryDetailModel model)
        {
            return model == null ? null : new RepositoryModel
            {
                Name = model.Name,
                Description = model.Description,
                Users = model.Users,
                Administrators = model.Administrators,
                Teams = model.Teams,
                AnonymousAccess = model.AllowAnonymous,
            };
        }
    }
}
