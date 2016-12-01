using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bonobo.Git.Server.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            string guid_regex = @"[\da-z]{8}-[\da-z]{4}-[\da-z]{4}-[\da-z]{4}-[\da-z]{12}";

            routes.MapRoute("SecureInfoRefs",
                            "{repositoryName}.git/info/refs",
                            new { controller = "Git", action = "SecureGetInfoRefs" },
                            new { method = new HttpMethodConstraint("GET") });

            routes.MapRoute("SecureUploadPack",
                            "{repositoryName}.git/git-upload-pack",
                            new { controller = "Git", action = "SecureUploadPack" },
                            new { method = new HttpMethodConstraint("POST") });

            routes.MapRoute("SecureReceivePack",
                            "{repositoryName}.git/git-receive-pack",
                            new { controller = "Git", action = "SecureReceivePack" },
                            new { method = new HttpMethodConstraint("POST") });

            routes.MapRoute("GitBaseUrl",
                            "{repositoryName}.git",
                            new { controller = "Git", action = "GitUrl" },
                            new { method = new HttpMethodConstraint("GET") });

            routes.MapRoute("IndexRoute", 
                            "{controller}/Index/",
                            new { action = "Index" });

            routes.MapRoute("CreateRoute", 
                            "{controller}/Create/",
                            new { action = "Create" });

            routes.MapRoute("RepositoryTree",
                            "Repository/{id}/Tree/{encodedName}/{*encodedPath}",
                            new { controller = "Repository", action = "Tree" },
                            new { id = guid_regex });

            routes.MapRoute("RepositoryBlob",
                            "Repository/{id}/{encodedName}/Blob/{*encodedPath}",
                            new { controller = "Repository", action = "Blob" },
                            new { id = guid_regex });

            routes.MapRoute("RepositoryRaw",
                            "Repository/{id}/{encodedName}/Raw/{*encodedPath}",
                            new { controller = "Repository", action = "Raw" },
                            new { id = guid_regex });

            routes.MapRoute("RepositoryBlame",
                            "Repository/{id}/{encodedName}/Blame/{*encodedPath}",
                            new { controller = "Repository", action = "Blame" },
                            new { id = guid_regex });

            routes.MapRoute("RepositoryDownload",
                            "Repository/{id}/{encodedName}/Download/{*encodedPath}",
                            new { controller = "Repository", action = "Download" },
                            new { id = guid_regex });

            routes.MapRoute("RepositoryCommits",
                            "Repository/{id}/{encodedName}/Commits",
                            new { controller = "Repository", action = "Commits" },
                            new { id = guid_regex });

            routes.MapRoute("RepositoryCommit",
                            "Repository/{id}/Commit/{commit}/",
                            new { controller = "Repository", action = "Commit" },
                            new { id = guid_regex });

            routes.MapRoute("RepositoryHistory",
                "Repository/{id}/{encodedName}/History/{*encodedPath}",
                new { controller = "Repository", action = "History" },
                            new { id = guid_regex });

            routes.MapRoute("Repository", 
                            "Repository/{id}/{action}/{reponame}",
                            new { controller = "Repository", action = "Detail", reponame = UrlParameter.Optional },
                            new { id = guid_regex });

            routes.MapRoute("Account",
                            "Account/{id}/{action}/{username}",
                            new { controller = "Account", action = "Detail", username = UrlParameter.Optional },
                            new { id = guid_regex });

            routes.MapRoute("Team", 
                            "Team/{id}/{action}/{teamname}",
                            new { controller = "Team", action = "Detail", teamname = UrlParameter.Optional },
                            new { id = guid_regex });


            routes.MapRoute("Validation", "Validation/{action}", new { controller = "Validation", action = String.Empty });

            routes.MapRoute("RepoCommits",
                            "Repository/{id}/Commits",
                            new { controller = "Repository", action = "Commits", id = string.Empty, page = 1 });

            routes.MapRoute("Default", 
                            "{controller}/{action}/{id}",
                            new { controller = "Home", action = "Index", id = String.Empty });

            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }
    }
}