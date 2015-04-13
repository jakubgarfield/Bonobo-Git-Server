using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Extensions;
using LibGit2Sharp;
using System.IO;
using Bonobo.Git.Server.Helpers;

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

        public IEnumerable<RepositoryTreeDetailModel> BrowseTree(string name, string path, out string referenceName, bool includeDetails = false)
        {
            var commit = GetCommitByName(name, out referenceName);
            if (commit == null)
            {
                return Enumerable.Empty<RepositoryTreeDetailModel>();
            }

            string branchName = referenceName ?? name;

            Tree tree;
            if (String.IsNullOrEmpty(path))
            {
                tree = commit.Tree;
            }
            else
            {
                var treeEntry = commit[path];
                if (treeEntry.TargetType == TreeEntryTargetType.Blob)
                {
                    return new[] { CreateRepositoryDetailModel(treeEntry, null, referenceName) };
                }

                if (treeEntry.TargetType == TreeEntryTargetType.GitLink)
                {
                    return new RepositoryTreeDetailModel[0];
                }

                tree = (Tree)treeEntry.Target;
            }

            return includeDetails ? GetTreeModelsWithDetails(commit, tree, branchName) : GetTreeModels(tree, branchName);
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
                IsLink = false,
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
            model.IsMarkdown = model.IsText && Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase);

            // try to render as text file if the extension matches
            if (FileDisplayHandler.GetBrush(path) != "plain")
            {
                model.IsText = true;
                model.Encoding = Encoding.Default;
                model.Text = new StreamReader(new MemoryStream(model.Data), model.Encoding, true).ReadToEnd();
            }

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

        public RepositoryBlameModel GetBlame(string name, string path, out string referenceName)
        {
            var modelBlob = BrowseBlob(name, path, out referenceName);
            if (modelBlob == null || !modelBlob.IsText)
            {
                return null;
            }
            var commit = GetCommitByName(name, out referenceName);
            string[] lines = modelBlob.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            List<RepositoryBlameHunkModel> hunks = new List<RepositoryBlameHunkModel>();
            foreach (var hunk in _repository.Blame(path, new BlameOptions { StartingAt = commit }))
            {
                hunks.Add(new RepositoryBlameHunkModel
                {
                    Commit = ToModel(hunk.FinalCommit),
                    Lines = lines.Skip(hunk.FinalStartLineNumber).Take(hunk.LineCount).ToArray()
                });
            }
            return new RepositoryBlameModel
            {
                Name = commit[path].Name,
                Path = path,
                Hunks = hunks,
                FileSize = modelBlob.Data.LongLength,
                LineCount = lines.LongLength
            };
        }

        public IEnumerable<RepositoryCommitModel> GetHistory(string path, string name, out string referenceName)
        {
            var commit = GetCommitByName(name, out referenceName);
            if (commit == null || String.IsNullOrEmpty(path))
            {
                return Enumerable.Empty<RepositoryCommitModel>();
            }

            return _repository.Commits
                              .QueryBy(new CommitFilter { Since = commit, SortBy = CommitSortStrategies.Topological })
                              .Where(c => c.Parents.Count() < 2 && c[path] != null && (c.Parents.Count() == 0 || c.Parents.FirstOrDefault()[path] == null || c[path].Target.Id != c.Parents.FirstOrDefault()[path].Target.Id))
                              .Select(s => ToModel(s)).ToList();
        }

        public void Dispose()
        {
            if (_repository != null)
            {
                _repository.Dispose();
            }
        }


        private IEnumerable<RepositoryTreeDetailModel> GetTreeModelsWithDetails(Commit commit, IEnumerable<TreeEntry> tree, string referenceName)
        {
            var ancestors = _repository.Commits.QueryBy(new CommitFilter { Since = commit, SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Reverse }).ToList();
            var entries = tree.ToList();
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
                        result.Add(CreateRepositoryDetailModel(entry, ancestor, referenceName));
                        entries[j] = null;
                    }
                }

                entries.RemoveAll(entry => entry == null);
            }

            return result;
        }

        private IEnumerable<RepositoryTreeDetailModel> GetTreeModels(IEnumerable<TreeEntry> tree, string referenceName)
        {
            return tree.Select(i => CreateRepositoryDetailModel(i, null, referenceName)).ToList();
        }

        private RepositoryTreeDetailModel CreateRepositoryDetailModel(TreeEntry entry, Commit ancestor, string treeName)
        {
            var maximumMessageLength = 50; //FIXME Propbably in appSettings?
            var originMessage = ancestor != null ? ancestor.Message : String.Empty;
            var commitMessage = !String.IsNullOrEmpty(originMessage)
                ? RepositoryCommitModelHelpers.MakeCommitMessage(originMessage, maximumMessageLength).ShortTitle : String.Empty;

            return new RepositoryTreeDetailModel
            {
                Name = entry.Name,
                CommitDate = ancestor != null ? ancestor.Author.When.LocalDateTime : default(DateTime?),
                CommitMessage = commitMessage,
                Author = ancestor != null ? ancestor.Author.Name : String.Empty,
                IsTree = entry.TargetType == TreeEntryTargetType.Tree,
                IsLink = entry.TargetType == TreeEntryTargetType.GitLink,
                TreeName = treeName,
                Path = entry.Path.Replace('\\', '/'),
                IsImage = FileDisplayHandler.IsImage(entry.Name),
            };
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
            string tagsString = string.Empty;
            var tags = _repository.Tags.Where(o => o.Target.Sha == commit.Sha);

            if (tags != null && tags.Any())
                tagsString = tags.Select(o => o.Name).Aggregate((o, p) => o + " " + p);

            var shortMessageDetails = RepositoryCommitModelHelpers.MakeCommitMessage(commit.Message, 30);

            var model = new RepositoryCommitModel
            {
                Author = commit.Author.Name,
                AuthorEmail = commit.Author.Email,
                AuthorAvatar = commit.Author.GetAvatar(),
                Date = commit.Author.When.LocalDateTime,
                ID = commit.Sha,
                Message = shortMessageDetails.ShortTitle,
                MessageShort = shortMessageDetails.ExtraTitle,
                TreeID = commit.Tree.Sha,
                Parents = commit.Parents.Select(i => i.Sha).ToArray(),
                TagName = tagsString,
                Notes = (from n in commit.Notes select new RepositoryCommitNoteModel(n.Message, n.Namespace)).ToList(),
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
                    ChangeId = i.Oid.Sha,
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