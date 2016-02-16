using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Helpers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Ionic.Zip;
using Microsoft.Practices.Unity;
using MimeTypes;

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

        [Dependency]
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        [WebAuthorize]
        public ActionResult Index(string sortGroup = null, string searchString = null)
        {
            var firstList = this.GetIndexModel();
            if (!string.IsNullOrEmpty(searchString))
            {
                var search = searchString.ToLower();
                firstList = firstList.Where(a => a.Name.ToLower().Contains(search) ||
                                            (!string.IsNullOrEmpty(a.Group) && a.Group.ToLower().Contains(search)) ||
                                            (!string.IsNullOrEmpty(a.Description) && a.Description.ToLower().Contains(search)))
                                            .AsEnumerable();
            }

            var list = firstList
                    .GroupBy(x => x.Group)
                    .OrderBy(x => x.Key, string.IsNullOrEmpty(sortGroup) || sortGroup.Equals("ASC"))
                    .ToDictionary(x => x.Key ?? string.Empty, x => x.ToArray());

            return View(list);
        }

        [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
        public ActionResult Edit(Guid id)
        {
            var model = ConvertRepositoryModel(RepositoryRepository.GetRepository(id));
            PopulateCheckboxListData(ref model);
            return View(model);
        }

        [HttpPost]
        [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
        public ActionResult Edit(RepositoryDetailModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.PostedSelectedAdministrators.Contains(User.Id()))
                {
                    RepositoryRepository.Update(ConvertRepositoryDetailModel(model));
                    ViewBag.UpdateSuccess = true;
                }
                else
                {
                    ModelState.AddModelError("Administrators", Resources.Repository_Edit_CantRemoveYourself);
                }
            }
            PopulateCheckboxListData(ref model);
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
                Administrators = new UserModel[] { MembershipService.GetUserModel(User.Id()) },
            };
            PopulateCheckboxListData(ref model);
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

            if (model != null && String.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("Name", Resources.Repository_Create_NameFailure);
            }
            else if (ModelState.IsValid)
            {
                if (RepositoryRepository.Create(ConvertRepositoryDetailModel(model)))
                {
                    string path = Path.Combine(UserConfiguration.Current.Repositories, model.Name);
                    if (!Directory.Exists(path))
                    {
                        LibGit2Sharp.Repository.Init(path, true);
                        TempData["CreateSuccess"] = true;
                        TempData["SuccessfullyCreatedRepositoryName"] = model.Name;
                        TempData["SuccessfullyCreatedRepositoryId"] = RepositoryRepository.GetRepository(model.Id).Id;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        RepositoryRepository.Delete(model.Id);
                        ModelState.AddModelError("", Resources.Repository_Create_DirectoryExists);
                    }
                }
                else
                {
                    ModelState.AddModelError("", Resources.Repository_Create_Failure);
                }
            }
            PopulateCheckboxListData(ref model);
            return View(model);
        }

        [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
        public ActionResult Delete(Guid id)
        {
            return View(ConvertRepositoryModel(RepositoryRepository.GetRepository(id)));
        }

        [HttpPost]
        [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
        public ActionResult Delete(RepositoryDetailModel model)
        {
            if (model != null)
            {
                var repo = RepositoryRepository.GetRepository(model.Id);
                string path = Path.Combine(UserConfiguration.Current.Repositories, repo.Name);
                if (Directory.Exists(path))
                {
                    DeleteFileSystemInfo(new DirectoryInfo(path));
                }
                RepositoryRepository.Delete(repo.Id);
                TempData["DeleteSuccess"] = true;
            }
            return RedirectToAction("Index");
        }

        [WebAuthorizeRepository]
        public ActionResult Detail(Guid id)
        {
            ViewBag.ID = id;
            var model = ConvertRepositoryModel(RepositoryRepository.GetRepository(id));
            if (model != null)
            {
                model.IsCurrentUserAdministrator = User.IsInRole(Definitions.Roles.Administrator) || RepositoryPermissionService.IsRepositoryAdministrator(User.Username(), model.Name);
                SetGitUrls(model);
            }
            using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, model.Name)))
            {
                string defaultReferenceName;
                browser.BrowseTree(null, null, out defaultReferenceName);
                RouteData.Values.Add("encodedName", defaultReferenceName);
            }

            return View(model);
        }

        /// <summary>
        /// Construct the URLs for the repository
        /// (This code extracted from the view)
        /// </summary>
        void SetGitUrls(RepositoryDetailModel model)
        {
            string serverAddress = System.Configuration.ConfigurationManager.AppSettings["GitServerPath"]
                                   ?? string.Format("{0}://{1}{2}{3}/",
                                       Request.Url.Scheme,
                                       Request.Url.Host,
                                       (Request.Url.IsDefaultPort ? "" : (":" + Request.Url.Port)),
                                       Request.ApplicationPath == "/" ? "" : Request.ApplicationPath
                                       );

            model.GitUrl = String.Concat(serverAddress, model.Name, ".git");
            if (User.Identity.IsAuthenticated)
            {
                model.PersonalGitUrl =
                    String.Concat(serverAddress.Replace("://", "://" + Uri.EscapeDataString(User.Username()) + "@"), model.Name, ".git");
            }
        }

        [WebAuthorizeRepository]
        public ActionResult Tree(Guid id, string encodedName, string encodedPath)
        {
            bool includeDetails = Request.IsAjaxRequest(); 

            ViewBag.ID = id;
            var name = PathEncoder.Decode(encodedName);
            var path = PathEncoder.Decode(encodedPath);

            var repo = RepositoryRepository.GetRepository(id);

            using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repo.Name)))
            {
                string referenceName;
                var files = browser.BrowseTree(name, path, out referenceName, includeDetails).ToList();
                
                var readme = files.FirstOrDefault(x => x.Path.Equals("readme.md", StringComparison.OrdinalIgnoreCase));
                string readmeTxt = string.Empty;
                if (readme != null)
                {
                    string refereceName;
                    var blob = browser.BrowseBlob(name, readme.Path, out refereceName);
                    readmeTxt = blob.Text;
                }
                var model = new RepositoryTreeModel
                {
                    Name = repo.Name,
                    Branch = name,
                    Path = path,
                    Files = files.OrderByDescending(i => i.IsTree).ThenBy(i => i.Name), 
                    Readme = readmeTxt
                };

                if (includeDetails)
                {
                    return Json(model, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    PopulateBranchesData(browser, referenceName);
                    PopulateAddressBarData(path);
                    return View(model);
                }
            }
        }

        [WebAuthorizeRepository]
        public ActionResult Blob(Guid id, string encodedName, string encodedPath)
        {
            ViewBag.ID = id;
            var repo = RepositoryRepository.GetRepository(id);
            using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repo.Name)))
            {
                var name = PathEncoder.Decode(encodedName);
                var path = PathEncoder.Decode(encodedPath);
                string referenceName;
                var model = browser.BrowseBlob(name, path, out referenceName);
                PopulateBranchesData(browser, referenceName);
                PopulateAddressBarData(path);

                return View(model);
            }
        }

        [WebAuthorizeRepository]
        public ActionResult Raw(Guid id, string encodedName, string encodedPath, bool display = false)
        {
            ViewBag.ID = id;

            var repo = RepositoryRepository.GetRepository(id);
            using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repo.Name)))
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
                    return File(model.Data, MimeTypeMap.GetMimeType(Path.GetExtension(model.Name.ToLower())), model.Name);
                }
            }

            return HttpNotFound();
        }

        [WebAuthorizeRepository]
        public ActionResult Blame(Guid id, string encodedName, string encodedPath)
        {
            ViewBag.ID = id;
            ViewBag.ShowShortMessageOnly = true;
            var repo = RepositoryRepository.GetRepository(id);
            using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repo.Name)))
            {
                var name = PathEncoder.Decode(encodedName);
                var path = PathEncoder.Decode(encodedPath);
                string referenceName;
                var model = browser.GetBlame(name, path, out referenceName);
                PopulateBranchesData(browser, referenceName);
                PopulateAddressBarData(path);

                return View(model);
            }
        }

        [WebAuthorizeRepository]
        public ActionResult Download(Guid id, string encodedName, string encodedPath)
        {
            var name = PathEncoder.Decode(encodedName);
            var path = PathEncoder.Decode(encodedPath);

            Response.BufferOutput = false;
            Response.Charset = "";
            Response.ContentType = "application/zip";

            var repo = RepositoryRepository.GetRepository(id);
            string headerValue = ContentDispositionUtil.GetHeaderValue((name ?? repo.Name) + ".zip");
            Response.AddHeader("Content-Disposition", headerValue);

            using (var outputZip = new ZipFile())
            {
                outputZip.UseZip64WhenSaving = Zip64Option.Always;
                outputZip.AlternateEncodingUsage = ZipOption.AsNecessary;
                outputZip.AlternateEncoding = Encoding.Unicode;

                using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repo.Name)))
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
                if (item.IsLink)
                {
                    outputZip.AddDirectoryByName(Path.Combine(item.TreeName, item.Path));
                }
                else if (!item.IsTree)
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
        public ActionResult Commits(Guid id, string encodedName, int page = 1)
        {
            page = page >= 1 ? page : 1;
            
            ViewBag.ID = id;
            ViewBag.ShowShortMessageOnly = true;
            var repo = RepositoryRepository.GetRepository(id);
            using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repo.Name)))
            {
                var name = PathEncoder.Decode(encodedName);
                string referenceName;
                int totalCount;
                var commits = browser.GetCommits(name, page, 10, out referenceName, out totalCount);
                PopulateBranchesData(browser, referenceName);
                ViewBag.TotalCount = totalCount;
                return View(new RepositoryCommitsModel { Commits = commits, Name = repo.Name });
            }
        }

        [WebAuthorizeRepository]
        public ActionResult Commit(Guid id, string commit)
        {
            ViewBag.ID = id;
            ViewBag.ShowShortMessageOnly = false;
            var repo = RepositoryRepository.GetRepository(id);
            using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repo.Name)))
            {
                var model = browser.GetCommitDetail(commit);
                model.Name = repo.Name;
                return View(model);
            }
        }

        [WebAuthorize]
        public ActionResult Clone(Guid id)
        {
            if (!User.IsInRole(Definitions.Roles.Administrator) && !UserConfiguration.Current.AllowUserRepositoryCreation)
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            var model = ConvertRepositoryModel(RepositoryRepository.GetRepository(id));
            model.Name = "";
            PopulateCheckboxListData(ref model);
            ViewBag.ID = id;
            return View(model);
        }

        [HttpPost]
        [WebAuthorize]
        [WebAuthorizeRepository]
        public ActionResult Clone(Guid id, RepositoryDetailModel model)
        {
            if (!User.IsInRole(Definitions.Roles.Administrator) && !UserConfiguration.Current.AllowUserRepositoryCreation)
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            if (model != null && !String.IsNullOrEmpty(model.Name))
            {
                model.Name = Regex.Replace(model.Name, @"\s", "");
            }

            if (model != null && String.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("Name", Resources.Repository_Create_NameFailure);
            }
            else if (ModelState.IsValid)
            {
                if (RepositoryRepository.Create(ConvertRepositoryDetailModel(model)))
                {
                    string targetRepositoryPath = Path.Combine(UserConfiguration.Current.Repositories, model.Name);
                    if (!Directory.Exists(targetRepositoryPath))
                    {
                        var source_repo = RepositoryRepository.GetRepository(id);
                        string sourceRepositoryPath = Path.Combine(UserConfiguration.Current.Repositories, source_repo.Name);
                        
                        LibGit2Sharp.CloneOptions options = new LibGit2Sharp.CloneOptions()
                            {
                                IsBare = true,
                                Checkout = false
                            };

                        LibGit2Sharp.Repository.Clone(sourceRepositoryPath, targetRepositoryPath, options);

                        using (var repo = new LibGit2Sharp.Repository(targetRepositoryPath))
                        {
                            if (repo.Network.Remotes.Any(r => r.Name == "origin"))
                            {
                                repo.Network.Remotes.Remove("origin");
                            }
                        }

                        TempData["CloneSuccess"] = true;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        RepositoryRepository.Delete(model.Id);
                        ModelState.AddModelError("", Resources.Repository_Create_DirectoryExists);
                    }
                }
                else
                {
                    ModelState.AddModelError("", Resources.Repository_Create_Failure);
                }
            }

            ViewBag.ID = id;
            PopulateCheckboxListData(ref model);
            return View(model);
        }

        [WebAuthorizeRepository]
        public ActionResult History(Guid id, string encodedPath, string encodedName)
        {
            ViewBag.ID = id;
            ViewBag.ShowShortMessageOnly = true;
            var repo = RepositoryRepository.GetRepository(id);
            using (var browser = new RepositoryBrowser(Path.Combine(UserConfiguration.Current.Repositories, repo.Name)))
            {
                var path = PathEncoder.Decode(encodedPath);
                var name = PathEncoder.Decode(encodedName);
                string referenceName;
                var commits = browser.GetHistory(path, name, out referenceName);
                return View(new RepositoryCommitsModel { Commits = commits, Name = repo.Name });
            }
        }

        private void PopulateCheckboxListData(ref RepositoryDetailModel model)
        {
            model = !string.IsNullOrEmpty(model.Name) ? ConvertRepositoryModel(RepositoryRepository.GetRepository(model.Id)) : model;
            model.AllAdministrators = MembershipService.GetAllUsers().ToArray();
            model.AllUsers = MembershipService.GetAllUsers().ToArray();
            model.AllTeams = TeamRepository.GetAllTeams().ToArray();
            model.PostedSelectedAdministrators = new Guid[0];
            model.PostedSelectedUsers = new Guid[0];
            model.PostedSelectedTeams = new Guid[0];
        }

        [HttpPost]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        // This takes an irrelevant ID, because there isn't a good route
        // to RepositoryController for anything without an Id which isn't the Index action
        public ActionResult Rescan(string id)
        {
            new RepositorySynchronizer().Run();
            return RedirectToAction("Index");
        }

        private void PopulateAddressBarData(string path)
        {
            ViewData["path"] = path;
        }

        private void PopulateBranchesData(RepositoryBrowser browser, string referenceName)
        {
            ViewData["referenceName"] = referenceName;
            ViewData["branches"] = browser.GetBranches();
            ViewData["tags"] = browser.GetTags();
        }

        private IEnumerable<RepositoryDetailModel> GetIndexModel()
        {
            IEnumerable<RepositoryModel> repositoryModels;
            if (User.IsInRole(Definitions.Roles.Administrator))
            {
                repositoryModels = RepositoryRepository.GetAllRepositories();
            }
            else
            {
                var userTeams = TeamRepository.GetTeams(User.Id()).Select(i => i.Id).ToArray();
                repositoryModels = RepositoryRepository.GetPermittedRepositories(User.Id(), userTeams);
            }
            return repositoryModels.Select(ConvertRepositoryModel).ToList();
        }

        private RepositoryDetailModel ConvertRepositoryModel(RepositoryModel model)
        {
            return model == null ? null : new RepositoryDetailModel
            {
                Id = model.Id,
                Name = model.Name,
                Group = model.Group,
                Description = model.Description,
                Users = model.Users,
                Administrators = model.Administrators,
                Teams = model.Teams,
                IsCurrentUserAdministrator = model.Administrators.Select(x => x.Name).Contains(User.Username(), StringComparer.OrdinalIgnoreCase),
                AllowAnonymous = model.AnonymousAccess,
                Status = GetRepositoryStatus(model),
                AuditPushUser = model.AuditPushUser,
                Logo = new RepositoryLogoDetailModel(model.Logo),
            };
        }

        private RepositoryDetailStatus GetRepositoryStatus(RepositoryModel model)
        {
            string path = Path.Combine(UserConfiguration.Current.Repositories, model.Name);
            if (!Directory.Exists(path))
            {
                return RepositoryDetailStatus.Missing;
            }
            else
            {
                return RepositoryDetailStatus.Valid;
            }
        }

        private RepositoryModel ConvertRepositoryDetailModel(RepositoryDetailModel model)
        {
            return model == null ? null : new RepositoryModel
            {
                Id = model.Id,
                Name = model.Name,
                Group = model.Group,
                Description = model.Description,
                Users = model.PostedSelectedUsers != null ? model.PostedSelectedUsers.Select(x => MembershipService.GetUserModel(x)).ToArray() : new UserModel[0],
                Administrators = model.PostedSelectedAdministrators != null ? model.PostedSelectedAdministrators.Select(x => MembershipService.GetUserModel(x)).ToArray() : new UserModel[0],
                Teams = model.PostedSelectedTeams != null ? model.PostedSelectedTeams.Select(x => TeamRepository.GetTeam(x)).ToArray() : new TeamModel[0],
                AnonymousAccess = model.AllowAnonymous,
                AuditPushUser = model.AuditPushUser,
                Logo = model.Logo != null ? model.Logo.BinaryData : null,
                RemoveLogo = model.Logo != null && model.Logo.RemoveLogo
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
