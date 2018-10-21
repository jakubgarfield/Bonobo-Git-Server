using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;

namespace Bonobo.Git.Tools
{
    public class GitDataSource
    {
        public IQueryable<Repository> Repositories
        {
            get 
            {
                var baseFolder = ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"];
                var directoryInfo = new DirectoryInfo(baseFolder);

                var repos= from dir in directoryInfo.EnumerateDirectories("*", SearchOption.AllDirectories)
                           where Repository.IsValid(dir.FullName)
                           select Repository.Open(dir.FullName); 

                return repos.AsQueryable();
            }
        }

        public IQueryable<Graph> RepositoryGraph
        {
            get
            {
                var baseFolder = ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"];
                var directoryInfo = new DirectoryInfo(baseFolder);

                var repos = from dir in directoryInfo.EnumerateDirectories("*", SearchOption.AllDirectories)
                            where Repository.IsValid(dir.FullName)
                            select new Graph(Repository.Open(dir.FullName));

                return repos.AsQueryable();
            }
        }

//        public IQueryable<Branch> Branches { get { return null; } }

        public IQueryable<Commit> Commits { get { return null; } }

        public IQueryable<Tree> Trees { get { return null; } }

        public IQueryable<Blob> Blobs { get { return null; } }

        public IQueryable<BlobContent> BlobContents { get { return null; } }

        public IQueryable<Ref> Refs { get { return null; } }

        public IQueryable<GraphNode> GraphNodes { get { return null; } }

        public IQueryable<GraphLink> GraphLinks { get { return null; } }

    }
}