
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LibGit2Sharp;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook.Hooks
{
    public class AuditPusherToGitNotes : IHookReceivePack
    {
        private const string EMPTY_USER = "<anonymous>";
        private IGitRepositoryLocator repoLocator;
        private IHookReceivePack next;
        private Bonobo.Git.Server.Data.IRepositoryRepository repoConfig;

        public AuditPusherToGitNotes(IHookReceivePack next, IGitRepositoryLocator repoLocator, Bonobo.Git.Server.Data.IRepositoryRepository repoConfig)
        {
            this.next = next;
            this.repoLocator = repoLocator;
            this.repoConfig = repoConfig;
        }

        public void PostPackReceive(ParsedReceivePack receivePack)
        {
            var repo = repoConfig.GetRepository(receivePack.RepositoryName);
            if (repo.AuditPushUser == true)
            {
                var user = receivePack.PushedByUser;
                if (string.IsNullOrEmpty(user))
                {
                    user = EMPTY_USER;
                }

                var gitRepo = new Repository(repoLocator.GetRepositoryDirectoryPath(receivePack.RepositoryName).FullName);
                foreach (var refChange in receivePack.RefChanges)
                {
                    var affectedCommits = gitRepo.Commits.QueryBy(new CommitFilter()
                    {
                        Since = refChange.ToCommit,
                        Until = refChange.FromCommit,
                        SortBy = CommitSortStrategies.Topological
                    });

                    foreach (var commit in affectedCommits)
                    {
                        gitRepo.Notes.Add(
                            commit.Id,
                            user,
                            "pusher");
                    }
                }
            }

            next.PostPackReceive(receivePack);
        }

        public void PrePackReceive(ParsedReceivePack receivePack)
        {
            next.PrePackReceive(receivePack);
        }
    }
}