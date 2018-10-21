using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;

namespace Bonobo.Git.Graph
{
    public class GitDataSource
    {
        private string _DefaultRepositoriesDirectory{ get; set; }

        public GitDataSource(string DefaultRepositoriesDirectory)
        {
            _DefaultRepositoriesDirectory = DefaultRepositoriesDirectory;
        }
        
        public IQueryable<Repository> Repositories
        {
            get 
            {
                var directoryInfo = new DirectoryInfo(_DefaultRepositoriesDirectory);

                var repos= from dir in directoryInfo.EnumerateDirectories("*", SearchOption.AllDirectories)
                           where Repository.IsValid(dir.FullName)
                           select Repository.Open(dir.FullName, _DefaultRepositoriesDirectory); 

                return repos.AsQueryable();
            }
        }

        public IQueryable<Graph> RepositoryGraph
        {
            get
            {
                var directoryInfo = new DirectoryInfo(_DefaultRepositoriesDirectory);

                var repos = from dir in directoryInfo.EnumerateDirectories("*", SearchOption.AllDirectories)
                            where Repository.IsValid(dir.FullName)
                            select new Graph(Repository.Open(dir.FullName, _DefaultRepositoriesDirectory));

                return repos.AsQueryable();
            }
        }

        public IQueryable<Commit> Commits { get { return null; } }

        public IQueryable<Tree> Trees { get { return null; } }

        public IQueryable<Ref> Refs { get { return null; } }

        public IQueryable<GraphNode> GraphNodes { get { return null; } }

        public IQueryable<GraphLink> GraphLinks { get { return null; } }

    }
}