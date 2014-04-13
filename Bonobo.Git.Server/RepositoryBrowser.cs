using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Models;
using LibGit2Sharp;
using System.IO;

namespace Bonobo.Git.Server
{
    public sealed class RepositoryBrowser : IDisposable
    {
        private readonly Repository _repository;

        public RepositoryBrowser(string repositoryPath)
        {
            if (!Repository.IsValid(repositoryPath))
            {
                throw new ArgumentException("Repository is not valid.", "repositoryPath");
            }

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
            var commit = GetCommitByName(name, out referenceName);
            if (commit == null)
            {
                return Enumerable.Empty<RepositoryCommitModel>();
            }

            return _repository.Commits
                              .QueryBy(new CommitFilter { Since = commit, SortBy = CommitSortStrategies.Topological })
                              .Select(s => ToModel(s)).ToList();
        }

        public RepositoryCommitModel GetCommitDetail(string name)
        {
            string referenceName;
            var commit = GetCommitByName(name, out referenceName);
            return commit == null ? null : ToModel(commit, true);
        }

        public IEnumerable<RepositoryTreeDetailModel> BrowseTree(string name, string path, out string referenceName)
        {
            var commit = GetCommitByName(name, out referenceName);
            if (commit == null)
            {
                return Enumerable.Empty<RepositoryTreeDetailModel>();
            }

            var tree = String.IsNullOrEmpty(path) ? commit.Tree : (Tree)commit[path].Target;
            var tmpReferenceName = referenceName;
            
            return tree.Select(i => new RepositoryTreeDetailModel
            {
                Name = i.Name,
                IsTree = i.TargetType == TreeEntryTargetType.Tree,
                TreeName = tmpReferenceName ?? name,
                Path = i.Path.Replace('\\', '/'),
                IsImage = FileDisplayHandler.IsImage(i.Name),
            }).ToList();
        }

        public IEnumerable<RepositoryTreeDetailModel> BrowseTreeDetails(string name, string path, out string referenceName)
        {
            var commit = GetCommitByName(name, out referenceName);
            if (commit == null)
            {
                return Enumerable.Empty<RepositoryTreeDetailModel>();
            }

            var ancestors = _repository.Commits.QueryBy(new CommitFilter { Since = commit, SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Reverse }).ToList();
            var entries = (String.IsNullOrEmpty(path) ? commit.Tree : (Tree)commit[path].Target).ToList();
            var result = new List<RepositoryTreeDetailModel>();

            for (int i = 0; i < ancestors.Count && entries.Any(); i++)
            {
                var ancestor = ancestors[i];

                for (int j = 0; j < entries.Count; j++)
                {
                    var entry = entries[j];
                    var ancestorEntry = ancestor[entry.Path];
                    if (ancestorEntry != null && entry.Target.Sha == ancestorEntry.Target.Sha)
                    {
                        result.Add(new RepositoryTreeDetailModel
                        {
                            Name = entry.Name,
                            CommitDate = ancestor.Author.When.LocalDateTime,
                            CommitMessage = ancestor.MessageShort,
                            Author = ancestor.Author.Name,
                            IsTree = entry.TargetType == TreeEntryTargetType.Tree,
                            TreeName = referenceName ?? name,
                            Path = entry.Path.Replace('\\', '/'),
                            IsImage = FileDisplayHandler.IsImage(entry.Name),
                        });

                        entries[j] = null;
                    }
                }

                entries.RemoveAll(entry => entry == null);
            }

            return result;
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

            var entry = commit[path];
            if (entry == null)
            {
                return null;
            }

            var model = new RepositoryTreeDetailModel
            {
                Name = entry.Name,
                IsTree = false,
                CommitDate = commit.Author.When.LocalDateTime,
                CommitMessage = commit.Message,
                Author = commit.Author.Name,
                TreeName = referenceName ?? name,
                Path = path,
            };

            using (var memoryStream = new MemoryStream())
            {
                ((Blob)entry.Target).GetContentStream().CopyTo(memoryStream);
                model.Data = memoryStream.ToArray();
            }

            model.Text = FileDisplayHandler.GetText(model.Data);
            model.Encoding = FileDisplayHandler.GetEncoding(model.Data);
            model.IsText = model.Text != null;
            if (model.IsText)
            {
                model.TextBrush = FileDisplayHandler.GetBrush(path);
            }
            else
            {
                model.IsImage = FileDisplayHandler.IsImage(path);
            }

            return model;
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
            if (tag == null)
            {
                return _repository.Lookup(name) as Commit;
            }

            referenceName = tag.Name;
            return tag.Target as Commit;
        }

        private RepositoryCommitModel ToModel(Commit commit, bool withDiff = false)
        {
            var model = new RepositoryCommitModel
            {
                Author = commit.Author.Name,
                AuthorEmail = commit.Author.Email,
                Date = commit.Author.When.LocalDateTime,
                ID = commit.Sha,
                Message = commit.Message,
                TreeID = commit.Tree.Sha,
                Parents = commit.Parents.Select(i => i.Sha).ToArray(),
            };

            if (!withDiff)
            {
                return model;
            }

            TreeChanges changes = !commit.Parents.Any() ? _repository.Diff.Compare<TreeChanges>(null, commit.Tree) : _repository.Diff.Compare<TreeChanges>(commit.Parents.First().Tree, commit.Tree);
            Patch patches = !commit.Parents.Any() ? _repository.Diff.Compare<Patch>(null, commit.Tree) : _repository.Diff.Compare<Patch>(commit.Parents.First().Tree, commit.Tree);

            model.Changes = changes.OrderBy(s => s.Path).Select(i =>
            {
                var patch = patches[i.Path];
                return new RepositoryCommitChangeModel
                {
                    Path = i.Path.Replace('\\', '/'),
                    Status = i.Status,
                    LinesAdded = patch.LinesAdded,
                    LinesDeleted = patch.LinesDeleted,
                    Patch = patch.Patch,
                };
            });

            return model;
        }
    }
}