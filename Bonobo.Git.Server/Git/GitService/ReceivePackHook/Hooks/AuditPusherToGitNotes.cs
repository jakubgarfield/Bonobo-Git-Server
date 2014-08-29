using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook.Hooks
{
    public class AuditPusherToGitNotes : IHookReceivePack
    {
        private const string EMPTY_USER = "<anonymous>";
        private IGitRepositoryLocator repoLocator;
        private IHookReceivePack next;

        public AuditPusherToGitNotes(IHookReceivePack next, IGitRepositoryLocator repoLocator)
        {
            this.next = next;
            this.repoLocator = repoLocator;
        }

        public void PostPackReceive(ParsedRecievePack receivePack, IEnumerable<ReceivePackCommits> commitData)
        {
            var user = receivePack.PushedByUser;
            if(string.IsNullOrEmpty(user))
            {
                user = EMPTY_USER;
            }

            var gitRepo = new Repository(repoLocator.GetRepositoryDirectoryPath(receivePack.RepositoryName).FullName);
            foreach (var commitGroup in commitData)
            {
                foreach (var commit in commitGroup.Commits)
                {
                    gitRepo.Notes.Add(
                        commit.Id,
                        user,
                        "pusher");
                }
            }
            next.PostPackReceive(receivePack, commitData);
        }

        public void PrePackReceive(ParsedRecievePack receivePack)
        {
            next.PrePackReceive(receivePack);
        }
    }
}