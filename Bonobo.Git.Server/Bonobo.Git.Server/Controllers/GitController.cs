using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GitSharp;
using GitSharp.Core.Transport;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using System.Configuration;

namespace Bonobo.Git.Server.Controllers
{
    public class GitController : Controller
    {
        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }

        [BasicAuthorize]
        public ActionResult SecureGetInfoRefs(String project, String service)
        {
            if (RepositoryPermissionService.HasPermission(HttpContext.User.Identity.Name, project)
                || (RepositoryPermissionService.AllowsAnonymous(project)
                    && (String.Equals("git-upload-pack", service, StringComparison.InvariantCultureIgnoreCase) 
                        || UserConfigurationManager.AllowAnonymousPush)))
            {
                return GetInfoRefs(project, service);
            }
            else
            {
                return UnauthorizedResult();
            }
        }

        [HttpPost]
        [BasicAuthorize]
        public ActionResult SecureUploadPack(String project)
        {
            if (RepositoryPermissionService.HasPermission(HttpContext.User.Identity.Name, project)
                || (RepositoryPermissionService.AllowsAnonymous(project) && UserConfigurationManager.AllowAnonymousPush))
            {
                return ExecuteUploadPack(project);
            }
            else
            {
                return UnauthorizedResult();
            }
        }

        [HttpPost]
        [BasicAuthorize]
        public ActionResult SecureReceivePack(String project)
        {
            if (RepositoryPermissionService.HasPermission(HttpContext.User.Identity.Name, project)
                || RepositoryPermissionService.AllowsAnonymous(project))
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
            Response.ContentType = "application/x-git-receive-pack-result";
            SetNoCache();

            var directory = GetDirectoryInfo(project);
            if (GitSharp.Repository.IsValid(directory.FullName, true))
            {
                using (var repository = new GitSharp.Repository(directory.FullName))
                using (var pack = new ReceivePack(repository))
                {
                    pack.setBiDirectionalPipe(false);
                    pack.receive(GetInputStream(), Response.OutputStream, Response.OutputStream);
                }

                return new EmptyResult();
            }
            else
            {
                return new HttpNotFoundResult();
            }
        }

        private ActionResult ExecuteUploadPack(string project)
        {
            Response.ContentType = "application/x-git-upload-pack-result";
            SetNoCache();

            var directory = GetDirectoryInfo(project);
            if (GitSharp.Repository.IsValid(directory.FullName, true))
            {
                using (var repository = new GitSharp.Repository(directory.FullName))
                using (var pack = new UploadPack(repository))
                {
                    pack.setBiDirectionalPipe(false);
                    pack.Upload(GetInputStream(), Response.OutputStream, Response.OutputStream);
                }

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

            Response.ContentType = String.Format("application/x-{0}-advertisement", service);
            SetNoCache();
            Response.Write(FormatMessage(String.Format("# service={0}\n", service)));
            Response.Write(FlushMessage());

            var directory = GetDirectoryInfo(project);
            if (GitSharp.Repository.IsValid(directory.FullName, true))
            {
                using (var repository = new GitSharp.Repository(directory.FullName))
                {
                    if (String.Equals("git-receive-pack", service, StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (var pack = new ReceivePack(repository))
                        {
                            pack.SendAdvertisedRefs(new RefAdvertiser.PacketLineOutRefAdvertiser(new PacketLineOut(Response.OutputStream)));
                        }

                    }
                    else if (String.Equals("git-upload-pack", service, StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (var pack = new UploadPack(repository))
                        {
                            pack.SendAdvertisedRefs(new RefAdvertiser.PacketLineOutRefAdvertiser(new PacketLineOut(Response.OutputStream)));
                        }
                    }
                }

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
            return new DirectoryInfo(Path.Combine(UserConfigurationManager.Repositories, project));
        }

        private Stream GetInputStream()
        {
            if (Request.Headers["Content-Encoding"] == "gzip")
            {
                return new GZipInputStream(Request.InputStream);
            }
            return Request.InputStream;
        }

        private void SetNoCache()
        {
            Response.AddHeader("Expires", "Fri, 01 Jan 1980 00:00:00 GMT");
            Response.AddHeader("Pragma", "no-cache");
            Response.AddHeader("Cache-Control", "no-cache, max-age=0, must-revalidate");
        }
    }
}
