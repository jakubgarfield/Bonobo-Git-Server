namespace Bonobo.Git.Server.Controllers
{
    using System;
    using System.IO;
    using System.Web.Mvc;
    using Bonobo.Git.Server.Configuration;
    using Bonobo.Git.Server.Git;
    using Bonobo.Git.Server.Git.GitService;
    using Bonobo.Git.Server.Security;
    using Ionic.Zlib;
    using LibGit2Sharp;
    using Microsoft.Practices.Unity;

    [GitAuthorize]
    public class GitController : Controller
    {
        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }
        
        [Dependency]
        public IGitService GitService { get; set; }

        public ActionResult SecureGetInfoRefs(String project, String service)
        {
            if (this.RepositoryPermissionService.HasPermission(this.User.Identity.Name, project)
                || (this.RepositoryPermissionService.AllowsAnonymous(project)
                    && (String.Equals("git-upload-pack", service, StringComparison.OrdinalIgnoreCase)
                        || UserConfiguration.Current.AllowAnonymousPush)))
            {
                return this.GetInfoRefs(project, service);
            }
            else
            {
                return this.UnauthorizedResult();
            }
        }

        [HttpPost]
        public ActionResult SecureUploadPack(String project)
        {
            if (this.RepositoryPermissionService.HasPermission(this.User.Identity.Name, project)
                || this.RepositoryPermissionService.AllowsAnonymous(project))
            {
                return this.ExecuteUploadPack(project);
            }
            else
            {
                return this.UnauthorizedResult();
            }
        }

        [HttpPost]
        public ActionResult SecureReceivePack(String project)
        {
            if (this.RepositoryPermissionService.HasPermission(this.User.Identity.Name, project)
                || (this.RepositoryPermissionService.AllowsAnonymous(project) && UserConfiguration.Current.AllowAnonymousPush))
            {
                return this.ExecuteReceivePack(project);
            }
            else
            {
                return this.UnauthorizedResult();
            }
        }

        private ActionResult ExecuteReceivePack(string project)
        {
            var directory = GetDirectoryInfo(project);
            if (Repository.IsValid(directory.FullName))
            {
                return new GitCmdResult(
                    "application/x-git-receive-pack-result",
                    (outStream) =>
                    {
                        this.GitService.ExecuteGitReceivePack(
                            Guid.NewGuid().ToString("N"),
                            project,
                            GetInputStream(disableBuffer: true),
                            outStream);
                    });
            }
            else
            {
                return new HttpNotFoundResult();
            }
        }

        private ActionResult ExecuteUploadPack(string project)
        {
            var directory = GetDirectoryInfo(project);
            if (Repository.IsValid(directory.FullName))
            {
                return new GitCmdResult(
                    "application/x-git-upload-pack-result",
                    (outStream) =>
                    {
                        this.GitService.ExecuteGitUploadPack(
                            Guid.NewGuid().ToString("N"),
                            project,
                            this.GetInputStream(),
                            outStream);
                    });
            }
            else
            {
                return new HttpNotFoundResult();
            }
        }

        private ActionResult GetInfoRefs(String project, String service)
        {
            var directory = GetDirectoryInfo(project);
            if (Repository.IsValid(directory.FullName))
            {
                this.Response.StatusCode = 200;

                string contentType = String.Format("application/x-{0}-advertisement", service);
                string serviceName = service.Substring(4);
                string advertiseRefsContent = FormatMessage(String.Format("# service={0}\n", service)) + FlushMessage();

                return new GitCmdResult(
                    contentType,
                    (outStream) =>
                    {
                        this.GitService.ExecuteServiceByName(
                            Guid.NewGuid().ToString("N"),
                            project, 
                            serviceName, 
                            new ExecutionOptions() { AdvertiseRefs = true },
                            this.GetInputStream(),
                            outStream
                        );
                    }, 
                    advertiseRefsContent);
            }
            else
            {
                return new HttpNotFoundResult();
            }
        }

        private ActionResult UnauthorizedResult()
        {
            this.Response.Clear();
            this.Response.AddHeader("WWW-Authenticate", "Basic realm=\"Secure Area\"");
            
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

        private static DirectoryInfo GetDirectoryInfo(String project)
        {
            return new DirectoryInfo(Path.Combine(UserConfiguration.Current.Repositories, project));
        }

        private Stream GetInputStream(bool disableBuffer = false)
        {
            // For really large uploads we need to get a bufferless input stream and disable the max
            // request length.
            Stream requestStream = disableBuffer ?
                this.Request.GetBufferlessInputStream(disableMaxRequestLength: true) :
                this.Request.GetBufferedInputStream();

            return this.Request.Headers["Content-Encoding"] == "gzip" ?
                new GZipStream(requestStream, CompressionMode.Decompress) :
                requestStream;
        }
    }
}