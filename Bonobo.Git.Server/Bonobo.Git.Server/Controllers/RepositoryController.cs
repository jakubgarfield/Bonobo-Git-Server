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

        [FormsAuthorizeAttribute]
        public ActionResult Index()
        {
            return View(GetIndexModel());
        }

        [FormsAuthorizeRepository(RequiresRepositoryAdministrator = true)]
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
        [FormsAuthorizeRepository(RequiresRepositoryAdministrator = true)]
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

        [FormsAuthorizeAttribute]
        public ActionResult Create()
        {
            if (!User.IsInRole(Definitions.Roles.Administrator) && !UserConfigurationManager.AllowUserRepositoryCreation)
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            var model = new RepositoryDetailModel
            {
                Administrators = new string[] { User.Identity.Name },
            };
            PopulateEditData();
            return View(model);
        }

        [HttpPost]
        [FormsAuthorizeAttribute]
        public ActionResult Create(RepositoryDetailModel model)
        {
            if (!User.IsInRole(Definitions.Roles.Administrator) && !UserConfigurationManager.AllowUserRepositoryCreation)
            {
                return RedirectToAction("Unauthorized", "Home");
            }

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
                        TempData["CreateSuccess"] = true;
                        return RedirectToAction("Index");
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

        [FormsAuthorizeRepository(RequiresRepositoryAdministrator = true)]
        public ActionResult Delete(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                return View(new RepositoryDetailModel { Name = id });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [FormsAuthorizeRepository(RequiresRepositoryAdministrator = true)]
        public ActionResult Delete(RepositoryDetailModel model)
        {
            if (model != null && !String.IsNullOrEmpty(model.Name))
            {
                string path = Path.Combine(UserConfigurationManager.Repositories, model.Name);
                if (Directory.Exists(path))
                {
                    DeleteFileSystemInfo(new DirectoryInfo(path));
                }
                RepositoryRepository.Delete(model.Name);
                TempData["DeleteSuccess"] = true;
            }
            return RedirectToAction("Index");
        }

        [FormsAuthorizeRepository]
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

        [FormsAuthorizeRepository]
        public ActionResult Tree(string id, string name, string path)
        {
            ViewBag.ID = id;
            if (!String.IsNullOrEmpty(id))
            {
                bool download = false;
                if (path != null)
                {
                    if (path.EndsWith(".browse"))
                    {
                        path = path.Substring(0, path.Length - 7);
                    }
                    else if (path.EndsWith(".download"))
                    {
                        path = path.Substring(0, path.Length - 9);
                        download = true;
                    }
                }
                path = path != null ? path.Replace(".browse", "") : null;
                using (var browser = new RepositoryBrowser(Path.Combine(UserConfigurationManager.Repositories, id)))
                {
                    string branchName;
                    var files = browser.Browse(name, path, out branchName);
                    PopulateBranchesData(browser, branchName);
                    PopulateAddressBarData(name, path);
                    return DisplayFiles(files, path, id, download);
                }
            }
            return View();
        }

        [FormsAuthorizeRepository]
        public ActionResult Commits(string id, string name)
        {
            ViewBag.ID = id;
            if (!String.IsNullOrEmpty(id))
            {
                using (var browser = new RepositoryBrowser(Path.Combine(UserConfigurationManager.Repositories, id)))
                {
                    string currentBranchName;
                    var commits = browser.GetCommits(name, out currentBranchName);
                    PopulateBranchesData(browser, currentBranchName);
                    return View(new RepositoryCommitsModel { Commits = commits, Name = id });
                }
            }

            return View();
        }

        [FormsAuthorizeRepository]
        public ActionResult Commit(string id, string commit)
        {
            ViewBag.ID = id;
            if (!String.IsNullOrEmpty(id))
            {
                using (var browser = new RepositoryBrowser(Path.Combine(UserConfigurationManager.Repositories, id)))
                {
                    var model = browser.GetCommitDetail(commit);
                    model.Name = id;
                    return View(model);
                }
            }

            return View();
        }

        private ActionResult DisplayFiles(IEnumerable<RepositoryTreeDetailModel> files, string path, string id, bool returnAsBinary)
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
                    if (!returnAsBinary)
                    {
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
                    }
                    
                    if (!model.IsImage && !model.IsTextFile)
                    {
                        return File(model.File.Data, "application/octet-stream", model.File.Name);
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

        private static void DeleteFileSystemInfo(FileSystemInfo fsi)
        {
            fsi.Attributes = FileAttributes.Normal;
            var di = fsi as DirectoryInfo;

            if (di != null)
            {
                foreach (var dirInfo in di.GetFileSystemInfos())
                {
                    DeleteFileSystemInfo(dirInfo);
                }
            }

            fsi.Delete();
        }
    }
}
