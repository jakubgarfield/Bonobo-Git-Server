﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Helpers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Ionic.Zip;
using Microsoft.Practices.Unity;

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


        [WebAuthorize]
        public ActionResult Index()
        {
            return View(GetIndexModel());
        }

        [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
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
        [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
        public ActionResult Edit(RepositoryDetailModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Administrators.Contains(User.Identity.Name.ToLowerInvariant()))
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

        [WebAuthorize]
        public ActionResult Create()
        {
            if (!User.IsInRole(Definitions.Roles.Administrator) && !UserConfiguration.Current.AllowUserRepositoryCreation)
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            var model = new RepositoryDetailModel
            {
                Administrators = new string[] { User.Identity.Name.ToLowerInvariant() },
            };
            PopulateEditData();
            return View(model);
        }

        [HttpPost]
        [WebAuthorize]
        public ActionResult Create(RepositoryDetailModel model)
        {
            if (!User.IsInRole(Definitions.Roles.Administrator) && !UserConfiguration.Current.AllowUserRepositoryCreation)
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
                model.GitName = model.Name + ".git";
                if (RepositoryRepository.Create(ConvertRepositoryDetailModel(model)))
                {
                    string path = Path.Combine(UserConfiguration.Current.Repositories, model.GitName);
                    if (!Directory.Exists(path))
                    {
                        LibGit2Sharp.Repository.Init(path, true);
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

        [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
        public ActionResult Delete(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                return View(new RepositoryDetailModel { Name = id });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
        public ActionResult Delete(RepositoryDetailModel model)
        {
            if (model != null && !String.IsNullOrEmpty(model.Name))
            {
                string path = Path.Combine(UserConfiguration.Current.Repositories, model.Name);
                if (Directory.Exists(path))
                {
                    DeleteFileSystemInfo(new DirectoryInfo(path));
                }
                RepositoryRepository.Delete(model.Name);
                TempData["DeleteSuccess"] = true;
            }
            return RedirectToAction("Index");
        }

        [WebAuthorizeRepository]
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

        [WebAuthorizeRepository]
        public ActionResult Tree(string id, string encodedName, string encodedPath)
        {
            bool includeDetails = Request.IsAjaxRequest(); 

            if (String.IsNullOrEmpty(id))
                return View();

            ViewBag.ID = id;
            var name = PathEncoder.Decode(encodedName);
            var path = PathEncoder.Decode(encodedPath);

            RepositoryModel repository = RepositoryRepository.GetRepository(id);
            using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repository.GitName)))
            {
                string referenceName;
                var files = browser.BrowseTree(name, path, out referenceName, includeDetails);

                var model = new RepositoryTreeModel
                {
                    Name = id,
                    Branch = name,
                    Path = path,
                    Files = files.OrderByDescending(i => i.IsTree).ThenBy(i => i.Name),
                };

                if (includeDetails)
                {
                    return Json(model, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    PopulateBranchesData(browser, referenceName);
                    PopulateAddressBarData(name, path);
                    return View(model);
                }
            }
        }

        [WebAuthorizeRepository]
        public ActionResult Blob(string id, string encodedName, string encodedPath)
        {
            ViewBag.ID = id;
            if (!String.IsNullOrEmpty(id))
            {
                RepositoryModel repository = RepositoryRepository.GetRepository(id);
                using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repository.GitName)))
                {
                    var name = PathEncoder.Decode(encodedName);
                    var path = PathEncoder.Decode(encodedPath);
                    string referenceName;
                    var model = browser.BrowseBlob(name, path, out referenceName);
                    PopulateBranchesData(browser, referenceName);
                    PopulateAddressBarData(name, path);

                    return View(model);
                }
            }
            return View();
        }

        [WebAuthorizeRepository]
        public ActionResult Raw(string id, string encodedName, string encodedPath, bool display = false)
        {
            ViewBag.ID = id;
            if (String.IsNullOrEmpty(id))
                return HttpNotFound();

            RepositoryModel repository = RepositoryRepository.GetRepository(id);
            using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repository.GitName)))
            {
                var name = PathEncoder.Decode(encodedName);
                var path = PathEncoder.Decode(encodedPath);
                string referenceName;
                var model = browser.BrowseBlob(name, path, out referenceName);

                if (!display)
                {
                    return File(model.Data, "application/octet-stream", model.Name);
                }
                if (model.IsText)
                {
                    return Content(model.Text, "text/plain", model.Encoding);
                }
                if (model.IsImage)
                {
                    return File(model.Data, FileDisplayHandler.GetMimeType(model.Name), model.Name);
                }
            }

            return HttpNotFound();
        }

        [WebAuthorizeRepository]
        public ActionResult Download(string id, string encodedName, string encodedPath)
        {
            if (String.IsNullOrEmpty(id))
                return HttpNotFound();

            var name = PathEncoder.Decode(encodedName);
            var path = PathEncoder.Decode(encodedPath);

            Response.BufferOutput = false;
            Response.Charset = "";
            Response.ContentType = "application/zip";

            string headerValue = ContentDispositionUtil.GetHeaderValue((name ?? id) + ".zip");
            Response.AddHeader("Content-Disposition", headerValue);

            using (var outputZip = new ZipFile())
            {
                outputZip.UseZip64WhenSaving = Zip64Option.Always;
                outputZip.AlternateEncodingUsage = ZipOption.AsNecessary;
                outputZip.AlternateEncoding = Encoding.Unicode;

                RepositoryModel repository = RepositoryRepository.GetRepository(id);
                using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repository.GitName)))
                {
                    AddTreeToZip(browser, name, path, outputZip);
                }

                outputZip.Save(Response.OutputStream);

                return new EmptyResult();
            }
        }

        private static void AddTreeToZip(RepositoryBrowser browser, string name, string path, ZipFile outputZip)
        {
            string referenceName;
            var treeNode = browser.BrowseTree(name, path, out referenceName);

            foreach (var item in treeNode)
            {
                if (!item.IsTree)
                {
                    string blobReferenceName;
                    var model = browser.BrowseBlob(item.TreeName, item.Path, out blobReferenceName);
                    outputZip.AddEntry(Path.Combine(item.TreeName, item.Path), model.Data);
                }
                else
                {
                    // recursive call
                    AddTreeToZip(browser, item.TreeName, item.Path, outputZip);
                }
            }
        }

        [WebAuthorizeRepository]
        public ActionResult Commits(string id, string encodedName)
        {
            ViewBag.ID = id;
            if (!String.IsNullOrEmpty(id))
            {
                RepositoryModel repository = RepositoryRepository.GetRepository(id);
                using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repository.GitName)))
                {
                    var name = PathEncoder.Decode(encodedName);
                    string referenceName;
                    var commits = browser.GetCommits(name, out referenceName);
                    PopulateBranchesData(browser, referenceName);
                    return View(new RepositoryCommitsModel { Commits = commits, Name = id });
                }
            }

            return View();
        }

        [WebAuthorizeRepository]
        public ActionResult Commit(string id, string commit)
        {
            ViewBag.ID = id;
            if (!String.IsNullOrEmpty(id))
            {
                RepositoryModel repository = RepositoryRepository.GetRepository(id);
                using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repository.GitName)))
                {
                    var model = browser.GetCommitDetail(commit);
                    model.Name = id;
                    return View(model);
                }
            }

            return View();
        }


        private void PopulateAddressBarData(string name, string path)
        {
            ViewData["path"] = path;
            ViewData["name"] = name;
        }

        private void PopulateBranchesData(RepositoryBrowser browser, string referenceName)
        {
            ViewData["referenceName"] = referenceName;
            ViewData["branches"] = browser.GetBranches();
            ViewData["tags"] = browser.GetTags();
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
                GitName = model.GitName,
                Description = model.Description,
                Users = model.Users,
                Administrators = model.Administrators,
                Teams = model.Teams,
                IsCurrentUserAdministrator = model.Administrators.Contains(User.Identity.Name.ToLowerInvariant()),
                AllowAnonymous = model.AnonymousAccess,
                Status = GetRepositoryStatus(model)
            };
        }

        private RepositoryDetailStatus GetRepositoryStatus(RepositoryModel model)
        {
            string path = Path.Combine(UserConfiguration.Current.Repositories, model.GitName);
            if (!Directory.Exists(path))
                return RepositoryDetailStatus.Missing;
            else
                return RepositoryDetailStatus.Valid;
        }

        private RepositoryModel ConvertRepositoryDetailModel(RepositoryDetailModel model)
        {
            return model == null ? null : new RepositoryModel
            {
                Name = model.Name,
                GitName = model.GitName,
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
