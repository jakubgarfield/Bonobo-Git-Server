using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;
using LibGit2Sharp;

namespace Bonobo.Git.Server.Application.Hooks {
    /// <summary>
    ///     Adds Bonobo user specific identity information to all newly pushed commits by appending a 
    ///     Git note to them.
    /// </summary>
    public class AfterPushAuditHandler: IAfterGitPushHandler
    {
        /// <summary>
        ///     Fallback username if no database user is known for the pushing client.
        /// </summary>
        private const string EmptyUser = "anonymous";
        private const string EmptyEmail = "unknown";

        private readonly IRepositoryRepository _repoConfig;
        private readonly IMembershipService _bonoboUsers;

        public AfterPushAuditHandler(IRepositoryRepository repoConfig, IMembershipService bonoboUsers)
        {
            if (repoConfig == null) throw new ArgumentNullException(nameof(repoConfig));
            if (bonoboUsers == null) throw new ArgumentNullException(nameof(bonoboUsers));

            _repoConfig = repoConfig;
            _bonoboUsers = bonoboUsers;
        }

        public void OnBranchCreated(HttpContext httpContext, GitBranchPushData branchData)
        {
            HandleAnyNewCommits(httpContext, branchData);
        }

        public void OnBranchModified(HttpContext httpContext, GitBranchPushData branchData, bool isFastForward)
        {
            HandleAnyNewCommits(httpContext, branchData);
        }

        private void HandleAnyNewCommits(HttpContext httpContext, GitBranchPushData branchData)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (!IsRepoAuditEnabled(branchData.RepositoryName))
                return;

            string bonoboUserName = HttpContext.Current.User.Username();
            AddCommitNotes(bonoboUserName, branchData);
        }

        private bool IsRepoAuditEnabled(string repositoryName)
        {
            var repo = _repoConfig.GetRepository(repositoryName);
            return repo.AuditPushUser;
        }

        private void AddCommitNotes(string bonoboUserName, GitBranchPushData branchData)
        {
            string email = null;
            if (string.IsNullOrEmpty(bonoboUserName))
            {
                bonoboUserName = EmptyUser;
            } else {
                var bonoboUser = _bonoboUsers.GetUserModel(bonoboUserName);
                if (bonoboUser != null)
                    email = bonoboUser.Email;
            }

            if (string.IsNullOrWhiteSpace(email))
                email = EmptyEmail;

            foreach (var commit in branchData.AddedCommits)
            {
                branchData.Repository.Notes.Add(
                    commit.Id,
                    bonoboUserName,
                    new Signature(bonoboUserName, email, DateTimeOffset.Now),
                    new Signature(bonoboUserName, email, DateTimeOffset.Now),
                    "pusher");
            }
        }

        public void OnBranchDeleted(HttpContext httpContext, GitBranchPushData branchData) {}

        public void OnTagCreated(HttpContext httpContext, GitTagPushData tagData) {}

        public void OnTagDeleted(HttpContext httpContext, GitTagPushData tagData) {}
    }
}