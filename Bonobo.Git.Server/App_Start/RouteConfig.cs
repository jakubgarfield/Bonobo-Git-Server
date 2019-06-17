using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Bonobo.Git.Server.App_Start
{
    public class RouteConfig
    {
        public static IRouteBuilder RegisterRoutes(IRouteBuilder routes)
        {
            routes.MapRoute("SecureInfoRefs",
                            "{repositoryName}.git/info/refs",
                            new { controller = "Git", action = "SecureGetInfoRefs" },
                            new { method = new HttpMethodRouteConstraint("GET") });

            routes.MapRoute("SecureUploadPack",
                            "{repositoryName}.git/git-upload-pack",
                            new { controller = "Git", action = "SecureUploadPack" },
                            new { method = new HttpMethodRouteConstraint("POST") });

            routes.MapRoute("SecureReceivePack",
                            "{repositoryName}.git/git-receive-pack",
                            new { controller = "Git", action = "SecureReceivePack" },
                            new { method = new HttpMethodRouteConstraint("POST") });

            routes.MapRoute("GitBaseUrl",
                            "{repositoryName}.git",
                            new { controller = "Git", action = "GitUrl" },
                            new { method = new HttpMethodRouteConstraint("GET") });

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
                            new { controller = "Repository", action = "Blob" },
                            new { id = @"\d+" });

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
                new { controller = "Repository", action = "History" },
                            new { id = @"\d+" });

            routes.MapRoute("Repository",
                            "Repository/{id}/{action}/{reponame?}",
                            new { controller = "Repository", action = "Detail" },
                            new { id = @"\d+" });

            routes.MapRoute("Account",
                            "Account/{id}/{action}/{username?}",
                            new { controller = "Account", action = "Detail" },
                            new { id = @"\d+" });

            routes.MapRoute("Team",
                            "Team/{id}/{action}/{teamname?}",
                            new { controller = "Team", action = "Detail" },
                            new { id = @"\d+" });


            routes.MapRoute("Validation", "Validation/{action}",
                            new {controller = "Validation", action = string.Empty});

            routes.MapRoute("RepoCommits",
                            "Repository/Commits/{id?}",
                            new { controller = "Repository", action = "Commits"});

            routes.MapRoute("Default",
                            "{controller}/{action}/{id?}",
                            new { controller = "Home", action = "Index" });

            //routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            //routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            return routes;
        }
    }
}