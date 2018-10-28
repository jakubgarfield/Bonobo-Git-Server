using Bonobo.Git.Graph;
using Bonobo.Git.Server.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Controllers
{
    public class RepositoryGraphController : Controller
    {
        [WebAuthorizeRepository(AllowAnonymousAccessWhenRepositoryAllowsIt = true)]
        public ActionResult GetRepoGraph(string repositoryName)
        {
            Bonobo.Git.Graph.Graph result = null;

            GitDataSource git = new GitDataSource(Path.IsPathRooted(UserConfiguration.Current.RepositoryPath) ? UserConfiguration.Current.RepositoryPath : Server.MapPath(UserConfiguration.Current.RepositoryPath));
            var graph = git.RepositoryGraph.Where(p => p.Name == repositoryName).FirstOrDefault();
            if (graph != null)
                result = graph;

            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}
