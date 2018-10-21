using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web;
using System.IO;
using System.Configuration;

namespace Bonobo.Git.Tools
{
    public class GitWebVirtualPathProvider : VirtualPathProvider
    {
        private string baseFolder = ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"];
        
        public override bool FileExists(string virtualPath)
        {
            var filePath = GetProjectWebPath(virtualPath);
            
            System.Diagnostics.Trace.WriteLine(string.Format("{0}=>{1}:IsVirtual={2}",
                virtualPath, filePath, filePath!=null), "VirtualProvider");

            return filePath == null ? base.FileExists(virtualPath) : true;
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            var filePath = GetProjectWebPath(virtualPath);

            return filePath == null ? 
                base.GetFile(virtualPath) : new ProjectWebFile(filePath);
        }

        private string GetProjectWebPath(string virtualPath)
        {
            try
            {
                var path = HttpContext.Current.Server.MapPath(virtualPath);
                if (File.Exists(path)) return null; //Ignore physical files

                var fileExt = Path.GetExtension(virtualPath);
                if (fileExt == "" || fileExt == ".htm" || fileExt == ".html" || fileExt == ".css" ||
                    fileExt == ".jpg" || fileExt == ".js" || fileExt == ".png")
                {
                    virtualPath = virtualPath.Substring(virtualPath.IndexOf("/") + 1);
                    var ss = virtualPath.Split('/');
                    var filePath = Path.Combine(baseFolder, string.Join("\\", ss));
                    if (Path.GetExtension(virtualPath) == "") filePath = Path.Combine(filePath, "default.htm");

                    return File.Exists(filePath) ? filePath : null;
                }
            }
            catch { }
            return null;
        }

    }
}
