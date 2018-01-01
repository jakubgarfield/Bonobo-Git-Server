using System.Collections.Generic;
using LibGit2Sharp;

namespace Bonobo.Git.Server.Application.Hooks
{
    public struct GitBranchPushData {
        public string RepositoryName { get; set; }
        public Repository Repository { get; set; }

        /// <summary>
        ///     Full name of the ref, e.g. "refs/heads/master"
        /// </summary>
        public string RefName { get; set; }

        /// <summary>
        ///     Branch name as it was extracted from <see cref="RefName" />
        ///     e.g. "master" or "feature/foo"
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        ///     The commit referenced by the branch.
        /// </summary>
        /// <returns>
        ///     The 40 characters long SHA1 hash in hex.
        /// </returns>
        public string ReferenceCommit { get; set; }
        
        /// <summary>
        ///     Commits which were not previously referenced by the branch.
        /// </summary>
        /// <returns>
        ///     All commits of a newly pushed branch (including the commit the 
        ///     branch originated from) or modified commits of an existing branch. 
        ///     <c>null</c> if branch got deleted.
        /// </returns>
        public IEnumerable<Commit> AddedCommits { get; set; }
    }
}