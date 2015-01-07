using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bonobo.Git.Server.App_Start
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
                            "Repository/{id}/{encodedName}/Tree/{*encodedPath}",
                            new { controller = "Repository", action = "Tree" });

            routes.MapRoute("RepositoryBlob",
                            "Repository/{id}/{encodedName}/Blob/{*encodedPath}",
                            new { controller = "Repository", action = "Blob" });

            routes.MapRoute("RepositoryRaw",
                            "Repository/{id}/{encodedName}/Raw/{*encodedPath}",
                            new { controller = "Repository", action = "Raw" });

            routes.MapRoute("RepositoryBlame",
                            "Repository/{id}/{encodedName}/Blame/{*encodedPath}",
                            new { controller = "Repository", action = "Blame" });

            routes.MapRoute("RepositoryDownload",
                            "Repository/{id}/{encodedName}/Download/{*encodedPath}",
                            new { controller = "Repository", action = "Download" });

            routes.MapRoute("RepositoryCommits",
                            "Repository/{id}/{encodedName}/Commits",
                            new { controller = "Repository", action = "Commits" });

            routes.MapRoute("RepositoryCommit",
                            "Repository/{id}/{encodedName}/Commit/{commit}/",
                            new { controller = "Repository", action = "Commit" });

            routes.MapRoute("RepositoryHistory",
                "Repository/{id}/{encodedName}/History/{*encodedPath}",
                new { controller = "Repository", action = "History" });

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