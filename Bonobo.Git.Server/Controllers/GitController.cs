using System.Configuration;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using System;
using System.IO;
using System.Web.Mvc;
using Bonobo.Git.Server.Git.GitService;
using System.Web;
using Ionic.Zlib;
using Bonobo.Git.Server.Git;

namespace Bonobo.Git.Server.Controllers
{
    [GitAuthorize]
    public class GitController : Controller
    {
        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }
        
        [Dependency]
        public IGitService GitService { get; set; }

        public ActionResult SecureGetInfoRefs(String project, String service)
        {
            if (RepositoryPermissionService.HasPermission(User.Identity.Name, project)
                || (RepositoryPermissionService.AllowsAnonymous(project)
                    && (String.Equals("git-upload-pack", service, StringComparison.InvariantCultureIgnoreCase)
                        || UserConfiguration.Current.AllowAnonymousPush)))
            {
                return GetInfoRefs(project, service);
            }
            else
            {
                return UnauthorizedResult();
            }
        }

        [HttpPost]
        public ActionResult SecureUploadPack(String project)
        {
            if (RepositoryPermissionService.HasPermission(User.Identity.Name, project)
                || RepositoryPermissionService.AllowsAnonymous(project))
            {
                return ExecuteUploadPack(project);
            }
            else
            {
                return UnauthorizedResult();
            }
        }

        [HttpPost]
        public ActionResult SecureReceivePack(String project)
        {
            if (RepositoryPermissionService.HasPermission(User.Identity.Name, project)
                || (RepositoryPermissionService.AllowsAnonymous(project) && UserConfiguration.Current.AllowAnonymousPush))
            {
                return ExecuteReceivePack(project);
            }
            else
            {
                return UnauthorizedResult();
            }
        }


        private ActionResult ExecuteReceivePack(string project)
        {
            var directory = GetDirectoryInfo(project);
            if (LibGit2Sharp.Repository.IsValid(directory.FullName))
            {
                return new GitCmdResult("application/x-git-receive-pack-result", (outStream) =>
                {
                    GitService.ExecuteGitReceivePack(Guid.NewGuid().ToString("N"), project, GetInputStream(), outStream);
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
            if (LibGit2Sharp.Repository.IsValid(directory.FullName))
            {
                return new GitCmdResult("application/x-git-upload-pack-result", (outStream) =>
                {
                    GitService.ExecuteGitUploadPack(Guid.NewGuid().ToString("N"), project, GetInputStream(), outStream);
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
            if (LibGit2Sharp.Repository.IsValid(directory.FullName))
            {
                Response.StatusCode = 200;

                string contentType = String.Format("application/x-{0}-advertisement", service);
                string serviceName = service.Substring(4);
                string advertiseRefsContent = FormatMessage(String.Format("# service={0}\n", service)) + FlushMessage();

                return new GitCmdResult(contentType, (outStream) =>
                {
                    GitService.ExecuteServiceByName(
                        Guid.NewGuid().ToString("N"),
                        project, 
                        serviceName, 
                        new ExecutionOptions() { AdvertiseRefs = true },
                        GetInputStream(),
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
            Response.Clear();
            Response.AddHeader("WWW-Authenticate", "Basic realm=\"Secure Area\"");
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

        private Stream GetInputStream()
        {
            return this.Request.Headers["Content-Encoding"] == "gzip" ? new GZipStream(this.Request.InputStream, CompressionMode.Decompress) : this.Request.InputStream;
        }
    }
}