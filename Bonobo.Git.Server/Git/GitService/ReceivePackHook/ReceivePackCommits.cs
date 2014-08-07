using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public class ReceivePackCommits
    {
        public ReceivePackCommits(string refName, ICommitLog commits)
        {
            this.RefName = refName;
            this.Commits = commits;
        }

        public string RefName { get; private set; }

        public ICommitLog Commits { get; private set; }
    }
}