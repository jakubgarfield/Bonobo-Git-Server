using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Bonobo.Git.Server.Extensions;

namespace Bonobo.Git.Server.Controllers
{
    [GitAuthorize]
    public class GitController : Controller
    {
        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }


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
            Response.Charset = "";
            Response.ContentType = "application/x-git-receive-pack-result";
            SetNoCache();

            var directory = GetDirectoryInfo(project);
            if (LibGit2Sharp.Repository.IsValid(directory.FullName))
            {
                RunGitCmd("receive-pack", false, directory.FullName, GetInputStream(), Response.OutputStream);
                return new EmptyResult();
            }
            else
            {
                return new HttpNotFoundResult();
            }
        }

        private ActionResult ExecuteUploadPack(string project)
        {
            Response.Charset = "";
            Response.ContentType = "application/x-git-upload-pack-result";
            SetNoCache();

            var directory = GetDirectoryInfo(project);
            if (LibGit2Sharp.Repository.IsValid(directory.FullName))
            {
                RunGitCmd("upload-pack", false, directory.FullName, GetInputStream(), Response.OutputStream);
                return new EmptyResult();
            }
            else
            {
                return new HttpNotFoundResult();
            }
        }

        private ActionResult GetInfoRefs(String project, String service)
        {
            Response.StatusCode = 200;
            Response.Charset = "";

            Response.ContentType = String.Format("application/x-{0}-advertisement", service);
            SetNoCache();
            Response.Write(FormatMessage(String.Format("# service={0}\n", service)));
            Response.Write(FlushMessage());

            var directory = GetDirectoryInfo(project);

            if (LibGit2Sharp.Repository.IsValid(directory.FullName))
            {
                RunGitCmd(service.Substring(4), true, directory.FullName, GetInputStream(), Response.OutputStream);
                return new EmptyResult();
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

        private DirectoryInfo GetDirectoryInfo(String project)
        {
            return new DirectoryInfo(Path.Combine(UserConfiguration.Current.Repositories, project));
        }

        private Stream GetInputStream()
        {
            if (Request.Headers["Content-Encoding"] == "gzip")
            {
                return new GZipStream(Request.InputStream, CompressionMode.Decompress);
            }
            return Request.InputStream;
        }

        private void SetNoCache()
        {
            Response.AddHeader("Expires", "Fri, 01 Jan 1980 00:00:00 GMT");
            Response.AddHeader("Pragma", "no-cache");
            Response.AddHeader("Cache-Control", "no-cache, max-age=0, must-revalidate");
        }

        public void RunGitCmd(string serviceName, bool advertiseRefs, string workingDir, Stream inStream, Stream outStream)
        {
            var args = serviceName + " --stateless-rpc";
            if (advertiseRefs)
                args += " --advertise-refs";
            args += " \"" + workingDir + "\"";

            var gitPath = Path.IsPathRooted(ConfigurationManager.AppSettings["GitPath"]) 
                ? ConfigurationManager.AppSettings["GitPath"] 
                : System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["GitPath"]);
            var info = new System.Diagnostics.ProcessStartInfo(gitPath, args)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(UserConfiguration.Current.Repositories),
            };

            using (var process = System.Diagnostics.Process.Start(info))
            {
                inStream.CopyTo(process.StandardInput.BaseStream);
                process.StandardInput.Write('\0');

                var buffer = new byte[16*1024];
                int read;
                while ((read = process.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, read);
                    outStream.Flush();
                }

                process.WaitForExit();
            }
        }
    }
}