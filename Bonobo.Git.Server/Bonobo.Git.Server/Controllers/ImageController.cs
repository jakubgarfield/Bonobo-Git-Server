using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.IO;
using System.Text;

namespace Bonobo.Git.Server.Controllers
{
    public class ImageController : Controller
    {
        public ActionResult Show(string repository, string tree, string path)
        {
            using (var browser = new RepositoryBrowser(Path.Combine(UserConfigurationManager.Repositories, repository)))
            {
                var leaf = browser.GetLeaf(tree, path);
                if (leaf != null)
                {
                    return new FileStreamResult(new MemoryStream(leaf.RawData), FileDisplayHandler.GetMimeType(Path.GetFileName(path)));
                }
            }
            return null;
        }
    }
}