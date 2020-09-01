using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Diagnostics;
using Bonobo.Git.Server.Application.Hooks;
using LibGit2Sharp;
using Log = Serilog.Log;

namespace Bonobo.Git.Server.Git.GitService
{
    /// <summary>
    ///     Invokes <see cref="IAfterGitPushHandler" /> according to transmitted Git protocol data.
    /// </summary>
    /// <remarks>
    ///     This service must be registered <strong>before</strong> <see cref="GitServiceExecutor" />, i.e. it has to wrap it
    ///     directly or indirectly.
    /// </remarks>
    public class GitHandlerInvocationService: IGitService
    {
        private readonly IGitService _next;
        private readonly IAfterGitPushHandler _afterPushHandler;
        private readonly IGitRepositoryLocator _repoLocator;

        public GitHandlerInvocationService(IGitService next, IAfterGitPushHandler afterPushHandler, IGitRepositoryLocator repoLocator)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            if (afterPushHandler == null) throw new ArgumentNullException(nameof(afterPushHandler));
            if (repoLocator == null) throw new ArgumentNullException(nameof(repoLocator));

            _next = next;
            _afterPushHandler = afterPushHandler;
            _repoLocator = repoLocator;
        }

        public void ExecuteServiceByName(string correlationId, string repositoryName, string serviceName, ExecutionOptions options, Stream inStream, Stream outStream)
        {
            if (serviceName != "receive-pack")
            {
                _next.ExecuteServiceByName(correlationId, repositoryName, serviceName, options, inStream, outStream);
                return;
            }

            var inspectStream = new ReceivePackInspectStream(inStream);

            // this should actually run the git process and the push will be complete once this method returns
            _next.ExecuteServiceByName(correlationId, repositoryName, serviceName, options, inspectStream, outStream);

            // because the Git process has successfully finished, the request really shouldn't fail due to any exceptions thrown from here
            try
            {
                Debug.WriteLine(
                    $"{nameof(GitHandlerInvocationService)} has found {inspectStream.PackObjectCount} objects in the receive-pack stream.");

                DirectoryInfo repoDirectory = _repoLocator.GetRepositoryDirectoryPath(repositoryName);
                Debug.Assert(repoDirectory.Exists);

                using (Repository repository = new Repository(repoDirectory.FullName))
                    InvokeHandler(repositoryName, repository, inspectStream.PeekedCommands);
            }
            catch (Exception ex)
            {
                Log.Error($"Git after push handlers could not be invoked due to this exception:\n{ex}");
            }
        }

        private void InvokeHandler(string repositoryName, Repository repository, IEnumerable<GitReceiveCommand> commands)
        {
            Debug.Assert(repositoryName != null);
            Debug.Assert(repositoryName.Length > 0);
            Debug.Assert(repository != null);
            Debug.Assert(commands != null);

            foreach (var command in commands)
            {
                try
                {
                    if (command.RefType == GitRefType.Branch)
                        InvokeAccordingToBranchChange(repositoryName, repository, command);
                    else if (command.RefType == GitRefType.Tag)
                        InvokeAccordingToTagChange(repositoryName, repository, command);
                }
                catch (Exception ex)
                {
                    Log.Error($"{nameof(IAfterGitPushHandler)} implementation has thrown an exception:\n{ex}");
                }
            }
        }

        private void InvokeAccordingToBranchChange(string repositoryName, Repository repository, GitReceiveCommand command)
        {
            Debug.Assert(repositoryName != null);
            Debug.Assert(repository != null);

            switch (command.CommandType) {
                case GitProtocolCommand.Create:
                {
                    var eventData = new GitBranchPushData
                    {
                        RepositoryName = repositoryName,
                        Repository = repository,
                        BranchName = command.RefName,
                        RefName = command.FullRefName,
                        ReferenceCommit = command.NewSha1,
                        AddedCommits = BranchCommits(repository, command.RefName).ToList()
                    };
                    _afterPushHandler.OnBranchCreated(HttpContext.Current, eventData);

                    break;
                }
                case GitProtocolCommand.Delete:
                {
                    var evenData = new GitBranchPushData
                    {
                        RepositoryName = repositoryName,
                        Repository = repository,
                        BranchName = command.RefName,
                        RefName = command.FullRefName,
                        ReferenceCommit = command.OldSha1
                    };
                    _afterPushHandler.OnBranchDeleted(HttpContext.Current, evenData);

                    break;
                }
                case GitProtocolCommand.Modify: {
                    Branch branch = repository.Branches[command.RefName];
                    bool isFastForward = branch.Commits.Any(c => c.Sha == command.OldSha1);

                    IEnumerable<Commit> addedCommits = BranchCommits(repository, command.RefName)
                        .TakeWhile(c => c.Sha != command.OldSha1).ToList();

                    var eventData = new GitBranchPushData
                    {
                        RepositoryName = repositoryName,
                        Repository = repository,
                        BranchName = command.RefName,
                        RefName = command.FullRefName,
                        ReferenceCommit = command.NewSha1,
                        AddedCommits = addedCommits
                    };
                    _afterPushHandler.OnBranchModified(HttpContext.Current, eventData, isFastForward);

                    break;
                }
            }
        }

        private void InvokeAccordingToTagChange(string repositoryName, Repository repository, GitReceiveCommand command)
        {
            Debug.Assert(repositoryName != null);
            Debug.Assert(repository != null);

            switch (command.CommandType) {
                case GitProtocolCommand.Create:
                {
                    var eventData = new GitTagPushData
                    {
                        RepositoryName = repositoryName,
                        Repository = repository,
                        TagName = command.RefName,
                        RefName = command.FullRefName,
                        ReferenceCommitSha = command.NewSha1
                    };
                    _afterPushHandler.OnTagCreated(HttpContext.Current, eventData);

                    break;
                }
                case GitProtocolCommand.Delete:
                {
                    var eventData = new GitTagPushData
                    {
                        RepositoryName = repositoryName,
                        Repository = repository,
                        TagName = command.RefName,
                        RefName = command.FullRefName,
                        ReferenceCommitSha = command.OldSha1
                    };
                    _afterPushHandler.OnTagDeleted(HttpContext.Current, eventData);

                    break;
                }
            }
        }

        private static IEnumerable<Commit> BranchCommits(Repository repository, string branchName)
        {
            var filter = new CommitFilter
            {
                IncludeReachableFrom = branchName,
                SortBy = CommitSortStrategies.Topological,
                FirstParentOnly = true
            };
            
            return repository.Commits
                .QueryBy(filter)
                .Intersect(repository.Branches[branchName].Commits);
        }
    }
}