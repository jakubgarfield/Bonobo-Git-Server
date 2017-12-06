using System;
using System.IO;
using System.IO.Compression;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Git;
using Bonobo.Git.Server.Git.GitService;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Repository = LibGit2Sharp.Repository;

namespace Bonobo.Git.Server.Controllers
{
    [GitAuthorize(AuthenticationSchemes = "Basic", Policy = "Git")]
    [RepositoryNameNormalizer("repositoryName")]
    public class GitController : Controller
    {
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }
        public IRepositoryRepository RepositoryRepository { get; set; }
        public IMembershipService MembershipService { get; set; }
        public IGitService GitService { get; set; }

        public GitController(
            IRepositoryPermissionService repositoryPermissionService,
            IRepositoryRepository repositoryRepository,
            IMembershipService membershipService,
            IGitService gitService
            )
        {
            RepositoryPermissionService = repositoryPermissionService;
            RepositoryRepository = repositoryRepository;
            MembershipService = membershipService;
            GitService = gitService;
        }

        [HttpGet("{repositoryName}.git/info/refs", Name = "SecureInfoRefs")]
        public IActionResult SecureGetInfoRefs(String repositoryName, String service)
        {
            bool isPush = String.Equals("git-receive-pack", service, StringComparison.OrdinalIgnoreCase);

            if (!RepositoryIsValid(repositoryName))
            {
                // This isn't a real repo - but we might consider allowing creation
                if (isPush && UserConfiguration.Current.AllowPushToCreate)
                {
                    if (!RepositoryPermissionService.HasCreatePermission(User.Id()))
                    {
                        Log.Warning("GitC: User {UserId} is not allowed to do push-to-create", User.Id());
                        return UnauthorizedResult();
                    }
                    if (!TryCreateOnPush(repositoryName))
                    {
                        return UnauthorizedResult();
                    }
                }
                else
                {
                    return new NotFoundResult();
                }
            }

            var requiredLevel = isPush ? RepositoryAccessLevel.Push : RepositoryAccessLevel.Pull;
            if (RepositoryPermissionService.HasPermission(User.Id(), repositoryName, requiredLevel))
            {
                return GetInfoRefs(repositoryName, service);
            }
            else
            {
                Log.Warning("GitC: SecureGetInfoRefs unauth because User {UserId} doesn't have permission {Permission} on  repo {RepositoryName}",
                    User.Id(),
                    requiredLevel,
                    repositoryName);
                return UnauthorizedResult();
            }
        }

        [HttpPost("{repositoryName}.git/git-upload-pack", Name = "SecureUploadPack")]
        public IActionResult SecureUploadPack(String repositoryName)
        {
            if (!RepositoryIsValid(repositoryName))
            {
                return new NotFoundResult();
            }

            if (RepositoryPermissionService.HasPermission(User.Id(), repositoryName, RepositoryAccessLevel.Pull))
            {
                return ExecuteUploadPack(repositoryName);
            }
            else
            {
                return UnauthorizedResult();
            }
        }

        [HttpPost("{repositoryName}.git/git-receive-pack", Name = "SecureReceivePack")]
        public IActionResult SecureReceivePack(String repositoryName)
        {
            if (!RepositoryIsValid(repositoryName))
            {
                return new NotFoundResult();
            }

            if (RepositoryPermissionService.HasPermission(User.Id(), repositoryName, RepositoryAccessLevel.Push))
            {
                return ExecuteReceivePack(repositoryName);
            }
            else
            {
                return UnauthorizedResult();
            }
        }

        private bool TryCreateOnPush(string repositoryName)
        {
            DirectoryInfo directory = GetDirectoryInfo(repositoryName);
            if (directory.Exists)
            {
                // We can't create a new repo - there's already a directory with that name
                Log.Warning("GitC: Can't create {RepositoryName} - directory already exists", repositoryName);
                return false;
            }
            RepositoryModel repository = new RepositoryModel();
            repository.Name = repositoryName;
            if (!repository.NameIsValid)
            {
                // We don't like this name
                Log.Warning("GitC: Can't create '{RepositoryName}' - name is invalid", repositoryName);
                return false;
            }
            var user = MembershipService.GetUserModel(User.Id());
            repository.Description = "Auto-created by push for " + user.DisplayName;
            repository.AnonymousAccess = false;
            repository.Administrators = new[] { user };
            if (!RepositoryRepository.Create(repository))
            {
                // We can't add this to the repo store
                Log.Warning("GitC: Can't create '{RepositoryName}' - RepoRepo.Create failed", repositoryName);
                return false;
            }

            Repository.Init(Path.Combine(UserConfiguration.Current.Repositories, repository.Name), true);
            Log.Information("GitC: '{RepositoryName}' created", repositoryName);
            return true;
        }


        /// <summary>
        /// This is the action invoked if you browse to a .git URL
        /// We just redirect to the repo details page, which is basically what GitHub does
        /// </summary>
        [HttpGet("{repositoryName}.git", Name = "GitBaseUrl")]
        public IActionResult GitUrl(string repositoryName)
        {
            return RedirectPermanent(Url.Action("Detail", "Repository", new { id = repositoryName }));
        }

        private IActionResult ExecuteReceivePack(string repositoryName)
        {
            return new GitCmdResult(
                "application/x-git-receive-pack-result",
                (outStream) =>
                {
                    GitService.ExecuteGitReceivePack(
                        Guid.NewGuid().ToString("N"),
                        repositoryName,
                        GetInputStream(disableBuffer: true),
                        outStream);
                });
        }

        private IActionResult ExecuteUploadPack(string repositoryName)
        {
            return new GitCmdResult(
                "application/x-git-upload-pack-result",
                (outStream) =>
                {
                    GitService.ExecuteGitUploadPack(
                        Guid.NewGuid().ToString("N"),
                        repositoryName,
                        GetInputStream(),
                        outStream);
                });
        }

        private IActionResult GetInfoRefs(String repositoryName, String service)
        {
            Response.StatusCode = 200;

            string contentType = String.Format("application/x-{0}-advertisement", service);
            string serviceName = service.Substring(4);
            string advertiseRefsContent = FormatMessage(String.Format("# service={0}\n", service)) + FlushMessage();

            return new GitCmdResult(
                contentType,
                (outStream) =>
                {
                    GitService.ExecuteServiceByName(
                        Guid.NewGuid().ToString("N"),
                        repositoryName,
                        serviceName,
                        new ExecutionOptions() { AdvertiseRefs = true },
                        GetInputStream(),
                        outStream
                    );
                },
                advertiseRefsContent);
        }

        private IActionResult UnauthorizedResult()
        {
            //Response.Body.Clear();
            Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Bonobo Git\"");

            return new StatusCodeResult(401);
        }

        private static String FormatMessage(String input)
        {
            return (input.Length + 4).ToString("X").PadLeft(4, '0') + input;
        }

        private static String FlushMessage()
        {
            return "0000";
        }

        private static DirectoryInfo GetDirectoryInfo(String repositoryName)
        {
            return new DirectoryInfo(Path.Combine(UserConfiguration.Current.Repositories, repositoryName));
        }

        private static bool RepositoryIsValid(string repositoryName)
        {
            var directory = GetDirectoryInfo(repositoryName);
            var isValid = Repository.IsValid(directory.FullName);
            if (!isValid)
            {
                Log.Warning("GitC: Invalid repo {RepositoryName}", repositoryName);
            }
            return isValid;
        }

        private Stream GetInputStream(bool disableBuffer = false)
        {
            // For really large uploads we need to get a bufferless input stream and disable the max
            // request length.
            Stream requestStream = Request.Body;/*  disableBuffer ?
                Request.GetBufferlessInputStream(disableMaxRequestLength: true) :
                Request.GetBufferedInputStream();*/

            return Request.Headers["Content-Encoding"] == "gzip" ?
                new GZipStream(requestStream, CompressionMode.Decompress) :
                requestStream;
        }

        //protected override void OnException(ExceptionContext filterContext)
        //{
        //    Exception exception = filterContext.Exception;
        //    Log.Error(exception, "Error caught in GitController");
        //    filterContext.Result = new ContentResult { Content = exception.ToString() };

        //    filterContext.ExceptionHandled = true;

        //    filterContext.HttpContext.Response.Clear();
        //    filterContext.HttpContext.Response.StatusCode = 500;
        //    filterContext.HttpContext.Response.StatusDescription = "Exception in GitController";
        //    filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        //}
    }
}
