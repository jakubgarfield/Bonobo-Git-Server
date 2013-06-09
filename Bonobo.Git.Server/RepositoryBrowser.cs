using Bonobo.Git.Server.Models;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonobo.Git.Server
{
    public sealed class RepositoryBrowser : IDisposable
    {
        private Repository _repository;


        public RepositoryBrowser(string repositoryPath)
        {
            if (!Repository.IsValid(repositoryPath))
                throw new ArgumentException("Repository is not valid.", "repositoryPath");

            _repository = new Repository(repositoryPath);
        }


        public IEnumerable<string> GetBranches()
        {
            return _repository.Branches.Select(s => s.Name).ToList();
        }

        public IEnumerable<string> GetTags()
        {
            return _repository.Tags.Select(s => s.Name).ToList();
        }

        public IEnumerable<RepositoryCommitModel> GetCommits(string name, out string referenceName)
        {
            var result = new List<RepositoryCommitModel>();
            var commit = GetCommitByName(name, out referenceName);

            if (commit == null)
                return result;

            var ancestors = _repository.Commits.QueryBy(new Filter { Since = commit, SortBy = GitSortOptions.Topological });
            result.AddRange(ancestors.Select(s => ConvertToRepositoryCommitModel(s)));

            return result;
        }

        public RepositoryCommitModel GetCommitDetail(string name)
        {
            string referenceName;
            var commit = GetCommitByName(name, out referenceName);

            return commit == null ? null : ConvertToRepositoryCommitModel(commit, true);
        }

        public IEnumerable<RepositoryTreeDetailModel> BrowseTree(string name, string path, out string referenceName)
        {
            if (path == null)
            {
                path = String.Empty;
            }

            var result = new List<RepositoryTreeDetailModel>();
            var commit = GetCommitByName(name, out referenceName);

            if (commit == null)
            {
                return result;
            }

            var branchNameTemp = referenceName;
            var ancestors = _repository.Commits.QueryBy(new Filter { Since = commit, SortBy = GitSortOptions.Topological | GitSortOptions.Reverse });
            var tree = String.IsNullOrEmpty(path) ? commit.Tree : (Tree)commit[path].Target;

            var q = from item in tree
                    let lastCommit = ancestors.First(c =>
                    {
                        var entry = c[item.Path];
                        return entry != null && entry.Target == item.Target;
                    })
                    select new RepositoryTreeDetailModel
                    {
                        Name = item.Name,
                        IsTree = item.TargetType == TreeEntryTargetType.Tree,
                        CommitDate = lastCommit.Author.When.LocalDateTime,
                        CommitMessage = lastCommit.MessageShort,
                        Author = lastCommit.Author.Name,
                        TreeName = branchNameTemp ?? name,
                        Path = item.Path.Replace('\\', '/'),
                    };
            return q.ToList();
        }

        public RepositoryTreeDetailModel BrowseBlob(string name, string path, out string referenceName)
        {
            if (path == null)
            {
                path = String.Empty;
            }

            var commit = GetCommitByName(name, out referenceName);

            if (commit == null)
            {
                return null;
            }

            var tree = commit.Tree;
            var dirs = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            TreeEntry entry;
            foreach (var dir in dirs.Take(dirs.Length - 1))
            {
                entry = tree.FirstOrDefault(s => s.TargetType == TreeEntryTargetType.Tree && s.Name == dir);

                if (entry == null)
                {
                    return null;
                }
                tree = (Tree)entry.Target;
            }
            entry = tree.FirstOrDefault(s => s.TargetType == TreeEntryTargetType.Blob && s.Name == dirs.Last());
            if (entry == null)
            {
                return null;
            }

            var blob = (Blob)entry.Target;
            return new RepositoryTreeDetailModel
            {
                Name = dirs.Last(),
                IsTree = false,
                CommitDate = commit.Author.When.LocalDateTime,
                CommitMessage = commit.Message,
                Author = commit.Author.Name,
                TreeName = referenceName ?? name,
                Path = path,
                Data = blob.Content,
            };
        }

        public void Dispose()
        {
            if (_repository != null)
            {
                _repository.Dispose();
            }
        }


        private Commit GetCommitByName(string name, out string referenceName)
        {
            referenceName = null;

            if (string.IsNullOrEmpty(name))
            {
                referenceName = _repository.Head.Name;
                return _repository.Head.Tip;
            }

            var branch = _repository.Branches[name];
            if (branch != null && branch.Tip != null)
            {
                referenceName = branch.Name;
                return branch.Tip;
            }

            var tag = _repository.Tags[name];
            if (tag != null)
            {
                referenceName = tag.Name;
                return tag.Target as Commit;
            }

            return _repository.Lookup(name) as Commit;
        }

        private RepositoryCommitModel ConvertToRepositoryCommitModel(Commit commit, bool withDiff = false)
        {
            var model = new RepositoryCommitModel
            {
                Author = commit.Author.Name,
                AuthorEmail = commit.Author.Email,
                Date = commit.Author.When.LocalDateTime,
                ID = commit.Sha,
                Message = commit.MessageShort,
                TreeID = commit.Tree.Sha,
                Parents = commit.Parents.Select(i => i.Sha).ToArray(),
            };
            if (withDiff)
            {
                TreeChanges changes = !commit.Parents.Any() ? _repository.Diff.Compare(null, commit.Tree) : _repository.Diff.Compare(commit.Parents.First().Tree, commit.Tree);
                model.Changes = changes.OrderBy(s => s.Path).Select(i => new RepositoryCommitChangeModel
                {
                    Path = i.Path.Replace('\\', '/'),
                    Status = i.Status,
                });
            }
            return model;
        }
    }
}