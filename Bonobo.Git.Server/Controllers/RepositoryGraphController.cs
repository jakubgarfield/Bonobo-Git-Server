using Bonobo.Git.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Controllers
{
    public class RepositoryGraphController : Controller
    {
        [WebAuthorizeRepository(AllowAnonymousAccessWhenRepositoryAllowsIt = true)]
        public ActionResult GetRepoNodes(string repositoryName)
        {
            List<GraphNode> result = new List<GraphNode>();

            GitDataSource git = new GitDataSource("");
            var graph = git.RepositoryGraph.Where(p => p.Name == repositoryName).FirstOrDefault();
            if (graph != null)
                result = graph.Nodes.ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [WebAuthorizeRepository(AllowAnonymousAccessWhenRepositoryAllowsIt = true)]
        public ActionResult GetRepoLinks(string repositoryName)
        {
            List<GraphLink> result = new List<GraphLink>();

            GitDataSource git = new GitDataSource("");
            var graph = git.RepositoryGraph.Where(p => p.Name == repositoryName).FirstOrDefault();
            if (graph != null)
                result = graph.Links.ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}