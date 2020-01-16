using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Git;
using Bonobo.Git.Server.Git.GitService;
using Bonobo.Git.Server.Helpers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Ionic.Zlib;
using Microsoft.Practices.Unity;
using Serilog;
using Repository = LibGit2Sharp.Repository;

namespace Bonobo.Git.Server.Controllers
{
    [GitAuthorize]
    [RepositoryNameNormalizer("repositoryName")]
    public class GitController : Controller
    {
        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }

        [Dependency]
        public IRepositoryRepository RepositoryRepository { get; set; }

        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IGitService GitService { get; set; }

        public ActionResult SecureGetInfoRefs(String repositoryName, String service)
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
                    return new HttpNotFoundResult();
                }
            }

            
            var requiredLevel = isPush ? RepositoryAccessLevel.Push : RepositoryAccessLevel.Pull;
            if (RepositoryPermissionService.HasPermission(User.Id(), repositoryName, requiredLevel))
            {
                return GetInfoRefs(repositoryName, service);
            }

            Log.Warning("GitC: SecureGetInfoRefs unauth because User {UserId} doesn't have permission {Permission} on  repo {RepositoryName}", 
                User.Id(),
                requiredLevel,
                repositoryName);

            return UnauthorizedResult();
        }

        [HttpPost]
        public ActionResult SecureUploadPack(String repositoryName)
        {
            if (!RepositoryIsValid(repositoryName))
            {
                return new HttpNotFoundResult();
            }

            if (RepositoryPermissionService.HasPermission(User.Id(), repositoryName, RepositoryAccessLevel.Pull))
            {
                return ExecuteUploadPack(repositoryName);
            }

            return UnauthorizedResult();
        }

        [HttpPost]
        public ActionResult SecureReceivePack(String repositoryName)
        {
            if (!RepositoryIsValid(repositoryName))
            {
                return new HttpNotFoundResult();
            }

            if (!RepositoryPermissionService.HasPermission(User.Id(), repositoryName, RepositoryAccessLevel.Push))
                return UnauthorizedResult();

            var input = new MemoryStream();
            GetInputStream(true).CopyTo(input);
            input.Seek(0, SeekOrigin.Begin);

            return !UserIsAllowedToPushIntoMasterBranch(repositoryName, input) ? UnauthorizedResult() : ExecuteReceivePack(repositoryName, input);
        }

        private bool UserIsAllowedToPushIntoMasterBranch(string repositoryName, Stream input)
        {
            var userIsAdmin = RepositoryPermissionService.UserIsAdministratorInRepository(User.Id(), repositoryName);
            var branch = GetBranchName(input);
            return !branch.Equals("master", StringComparison.OrdinalIgnoreCase) || userIsAdmin;
        }

        private static string GetBranchName(Stream inStream)
        {
            var pattern = new byte[] { 0x72, 0x65, 0x66, 0x73, 0x2F, 0x68, 0x65, 0x61, 0x64, 0x73, 0x2F }; // = 'refs/heads/'
            var buffer = new byte[256];

            try
            {
                if (inStream.Read(buffer, 0, buffer.Length) == 0)
                    return string.Empty;

                inStream.Seek(0, SeekOrigin.Begin);

                var readFrom = ByteUtils.IndexOf(buffer, pattern, 60);
                if (readFrom > -1)
                {
                    readFrom += pattern.Length;
                    var name = new byte[128];
                    var index = 0;
                    while (buffer[readFrom] != 0 && (index < name.Length && readFrom < buffer.Length))
                    {
                        name[index++] = buffer[readFrom++];
                    }

                    var result = Encoding.ASCII.GetString(name, 0, index);
                    return result.Trim();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }

            return string.Empty;
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
            repository.Administrators = new[] {user};
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
        public ActionResult GitUrl(string repositoryName)
        {
            return RedirectPermanent(Url.Action("Detail", "Repository", new { id = repositoryName}));
        }

        private ActionResult ExecuteReceivePack(string repositoryName, Stream inStream)
        {
            return new GitCmdResult(
                "application/x-git-receive-pack-result",
                (outStream) =>
                {
                    GitService.ExecuteGitReceivePack(
                        Guid.NewGuid().ToString("N"),
                        repositoryName,
                        inStream,
                        outStream);
                });
        }

        private ActionResult ExecuteUploadPack(string repositoryName)
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

        private ActionResult GetInfoRefs(String repositoryName, String service)
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

        private ActionResult UnauthorizedResult()
        {
            Response.Clear();
            Response.AddHeader("WWW-Authenticate", "Basic realm=\"Bonobo Git\"");
            
            return new HttpStatusCodeResult(401);
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
            // For really large uploads we need to get a bufferless inStream stream and disable the max
            // request length.
            Stream requestStream = disableBuffer ?
                Request.GetBufferlessInputStream(disableMaxRequestLength: true) :
                Request.GetBufferedInputStream();

            return Request.Headers["Content-Encoding"] == "gzip" ?
                new GZipStream(requestStream, CompressionMode.Decompress) :
                requestStream;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Exception exception = filterContext.Exception;
            Log.Error(exception, "Error caught in GitController");
            filterContext.Result = new ContentResult { Content = exception.ToString() };

            filterContext.ExceptionHandled = true;

            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.HttpContext.Response.StatusDescription = "Exception in GitController";
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}
