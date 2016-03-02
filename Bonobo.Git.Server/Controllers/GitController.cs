using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Mvc;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Git;
using Bonobo.Git.Server.Git.GitService;
using Bonobo.Git.Server.Security;
using Ionic.Zlib;
using LibGit2Sharp;
using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Controllers
{
    [GitAuthorize]
    [RepositoryNameNormalizer("repositoryName")]
    public class GitController : Controller
    {
        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }
        
        [Dependency]
        public IGitService GitService { get; set; }

        public ActionResult SecureGetInfoRefs(String repositoryName, String service)
        {
            if (!RepositoryIsValid(repositoryName))
            {
                return new HttpNotFoundResult();
            }

            bool isPush = String.Equals("git-receive-pack", service, StringComparison.OrdinalIgnoreCase);

            var requiredLevel = isPush ? RepositoryAccessLevel.Push : RepositoryAccessLevel.Pull;

            if (RepositoryPermissionService.HasPermission(User.Id(), repositoryName, requiredLevel))
            {
                return GetInfoRefs(repositoryName, service);
            }
            else
            {
                return UnauthorizedResult();
            }
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
            else
            {
                return UnauthorizedResult();
            }
        }

        [HttpPost]
        public ActionResult SecureReceivePack(String repositoryName)
        {
            if (!RepositoryIsValid(repositoryName))
            {
                return new HttpNotFoundResult();
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

        /// <summary>
        /// This is the action invoked if you browse to a .git URL
        /// We just redirect to the repo details page, which is basically what GitHub does
        /// </summary>
        public ActionResult GitUrl(string repositoryName)
        {
            return RedirectPermanent(Url.Action("Detail", "Repository", new { id = repositoryName}));
        }

        private ActionResult ExecuteReceivePack(string repositoryName)
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
            return isValid;
        }

        private Stream GetInputStream(bool disableBuffer = false)
        {
            // For really large uploads we need to get a bufferless input stream and disable the max
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
            Trace.TraceError("{0}: Error caught in GitController {1}", DateTime.Now, exception);
            filterContext.Result = new ContentResult { Content = exception.ToString() };

            filterContext.ExceptionHandled = true;

            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.HttpContext.Response.StatusDescription = "Exception in GitController";
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}
