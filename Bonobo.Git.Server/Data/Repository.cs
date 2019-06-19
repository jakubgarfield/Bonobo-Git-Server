using Bonobo.Git.Server.App_GlobalResources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Bonobo.Git.Server.Data.ManyToMany;

namespace Bonobo.Git.Server.Data
{
    public enum RepositoryPushMode
    {
        [Display(ResourceType = typeof(Resources), Name = "No")]
        No = 0,
        [Display(ResourceType = typeof(Resources), Name = "Yes")]
        Yes,
        [Display(ResourceType = typeof(Resources), Name = "Global")]
        Global,
    }

    public partial class Repository
    {
        private ICollection<TeamRepository_Permission> _teams;
        private ICollection<UserRepository_Administrator> _administrators;
        private ICollection<UserRepository_Permission> _users;

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }
        public bool Anonymous { get; set; }
        public byte[] Logo { get; set; }
        public RepositoryPushMode AllowAnonymousPush { get; set; }

        public virtual ICollection<TeamRepository_Permission> Teams
        {
            get
            {
                return _teams ?? (_teams = new List<TeamRepository_Permission>());
            }
            set
            {
                _teams = value;
            }
        }

        public virtual ICollection<UserRepository_Administrator> Administrators
        {
            get
            {
                return _administrators ?? (_administrators = new List<UserRepository_Administrator>());
            }
            set
            {
                _administrators = value;
            }
        }

        public virtual ICollection<UserRepository_Permission> Users
        {
            get
            {
                return _users ?? (_users = new List<UserRepository_Permission>());
            }
            set
            {
                _users = value;
            }
        }

        public Repository()
        {
            LinksUseGlobal = true;
        }

        public bool AuditPushUser { get; set; }

        public string LinksRegex { get; set; }
        public string LinksUrl { get; set; }
        public bool LinksUseGlobal { get; set; }


        /// <summary>
        /// Correct a repository name have the same case as it has in the database
        /// If the repo is not in the database, then the name is returned unchanged
        /// </summary>
        public static string NormalizeRepositoryName(string incomingRepositoryName, IRepositoryRepository repositoryRepository)
        {
            // In the most common case, we're just going to find the repo straight off
            // This is fastest if it succeeds, but might be case-sensitive
            var knownRepos = repositoryRepository.GetRepository(incomingRepositoryName);
            if (knownRepos != null)
            {
                return knownRepos.Name;
            }

            // We might have a real repo, but it wasn't returned by GetRepository, because that's not 
            // guaranteed to be case insensitive (very difficult to assure this with EF, because it's the back
            // end which matters, not EF itself)
            // We'll try and check all repos in a slow but safe fashion
            knownRepos =
                repositoryRepository.GetAllRepositories()
                    .FirstOrDefault(
                        repo => repo.Name.Equals(incomingRepositoryName, StringComparison.OrdinalIgnoreCase));
            if (knownRepos != null)
            {
                // We've found it now
                return knownRepos.Name;
            }

            // We can't find this repo - it's probably invalid, but it's not
            // our job to worry about that
            return incomingRepositoryName;
        }

    }
}
