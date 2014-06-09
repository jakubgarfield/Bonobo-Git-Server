using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bonobo.Git.Server
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SecureInfoRefs",
                            "{project}.git/info/refs",
                            new { controller = "Git", action = "SecureGetInfoRefs" },
                            new { method = new HttpMethodConstraint("GET") });

            routes.MapRoute("SecureUploadPack", 
                            "{project}.git/git-upload-pack",
                            new { controller = "Git", action = "SecureUploadPack" },
                            new { method = new HttpMethodConstraint("POST") });

            routes.MapRoute("SecureReceivePack", 
                            "{project}.git/git-receive-pack",
                            new { controller = "Git", action = "SecureReceivePack" },
                            new { method = new HttpMethodConstraint("POST") });

            routes.MapRoute("IndexRoute", 
                            "{controller}/Index/",
                            new { action = "Index" });

            routes.MapRoute("CreateRoute", 
                            "{controller}/Create/",
                            new { action = "Create" });

            routes.MapRoute("RepositoryTree", 
                            "Repository/{id}/Tree/{encodedName}/{*encodedPath}",
                            new { controller = "Repository", action = "Tree" });

            routes.MapRoute("RepositoryBlob", 
                            "Repository/{id}/Blob/{encodedName}/{*encodedPath}",
                            new { controller = "Repository", action = "Blob" });

            routes.MapRoute("RepositoryRaw",
                            "Repository/{id}/Raw/{encodedName}/{*encodedPath}",
                            new { controller = "Repository", action = "Raw" });

            routes.MapRoute("RepositoryDownload",
                            "Repository/{id}/Download/{encodedName}/{*encodedPath}",
                            new { controller = "Repository", action = "Download" });

            routes.MapRoute("RepositoryCommits", 
                            "Repository/{id}/Commits/{encodedName}/",
                            new { controller = "Repository", action = "Commits" });

            routes.MapRoute("RepositoryCommit", 
                            "Repository/{id}/Commit/{commit}/",
                            new { controller = "Repository", action = "Commit" });

            routes.MapRoute("Repository", 
                            "Repository/{id}/{action}/",
                            new { controller = "Repository", action = "Detail" });

            routes.MapRoute("Account", 
                            "Account/{id}/{action}/",
                            new { controller = "Account", action = "Detail" });

            routes.MapRoute("Team", 
                            "Team/{id}/{action}/",
                            new { controller = "Team", action = "Detail" });


            routes.MapRoute("Default", 
                            "{controller}/{action}/{id}",
                            new { controller = "Home", action = "Index", id = String.Empty });

            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }
    }
}