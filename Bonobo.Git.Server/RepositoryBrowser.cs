﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Extensions;
using LibGit2Sharp;
using System.IO;
using Bonobo.Git.Server.Helpers;
using System.Text.RegularExpressions;
using Bonobo.Git.Server.Configuration;
using Serilog;

namespace Bonobo.Git.Server
{
    public sealed class RepositoryBrowser : IDisposable
    {
        private readonly Repository _repository;

        public RepositoryBrowser(string repositoryPath)
        {
            if (!Repository.IsValid(repositoryPath))
            {
                Log.Error("Invalid repo path {RespositoryPath}", repositoryPath);
                throw new ArgumentException("Repository is not valid.", "repositoryPath");
            }

            _repository = new Repository(repositoryPath);
        }

        public IEnumerable<string> GetBranches()
        {
            return _repository.Branches.Select(s => s.FriendlyName).ToList();
        }

        public IEnumerable<string> GetTags()
        {
            return _repository.Tags.Select(s => s.FriendlyName).OrderByDescending(s => s).ToList();
        }

        public IEnumerable<RepositoryCommitModel> GetCommits(string name, int page, int pageSize, out string referenceName, out int totalCount)
        {
            var commit = GetCommitByName(name, out referenceName);
            if (commit == null)
            {
                totalCount = 0;
                return Enumerable.Empty<RepositoryCommitModel>();
            }

            IEnumerable<Commit> commitLogQuery = this._repository.Commits
                .QueryBy(new CommitFilter { IncludeReachableFrom = commit, SortBy = CommitSortStrategies.Topological });

            totalCount = commitLogQuery.Count();

            if (page >= 1 && pageSize >= 1)
            {
                commitLogQuery = commitLogQuery.Skip((page - 1) * pageSize).Take(pageSize);
            }

            return commitLogQuery.Select(s => ToModel(s)).ToList();
        }


        internal IEnumerable<RepositoryCommitModel> GetTags(string name, int page, int p, out string referenceName, out int totalCount)
        {
            var commit = GetCommitByName(name, out referenceName);
            if (commit == null)
            {
                totalCount = 0;
                return Enumerable.Empty<RepositoryCommitModel>();
            }
            var tags = _repository.Tags;
            var commits = new HashSet<RepositoryCommitModel>(AnonymousComparer.Create<RepositoryCommitModel>((x, y) => x.ID == y.ID, obj => obj.ID.GetHashCode()));
            foreach (var tag in tags)
            {
                var c = _repository.Lookup(tag.Target.Id) as Commit;
                commits.Add(ToModel(c));

            }
            totalCount = commits.Count();

            return commits.OrderByDescending(x => x, (x, y) => x.Date.CompareTo(y.Date));
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

            Encoding encoding;
            if(FileDisplayHandler.TryGetEncoding(model.Data, out encoding))
            {
                model.Text = FileDisplayHandler.GetText(model.Data, encoding);
                model.Encoding = encoding;
                model.IsText = model.Text != null;
                model.IsMarkdown = model.IsText && Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase);
            }
            model.TextBrush = FileDisplayHandler.GetBrush(path);

            // try to render as text file if the extension matches
            if (model.TextBrush != FileDisplayHandler.NoBrush && !model.IsText)
            {
                model.IsText = true;
                model.Encoding = Encoding.Default;
                model.Text = new StreamReader(new MemoryStream(model.Data), model.Encoding, true).ReadToEnd();
            }

            //blobs can be images even when they are text files.(like svg, but it's not in out MIME table yet)
            model.IsImage = FileDisplayHandler.IsImage(path);

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
                TreeName =  referenceName,
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
                              .QueryBy(new CommitFilter { IncludeReachableFrom = commit, SortBy = CommitSortStrategies.Topological })
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
            var ancestors = _repository.Commits.QueryBy(new CommitFilter { IncludeReachableFrom = commit, SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Reverse }).ToList();
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
                referenceName = _repository.Head.FriendlyName;
                return _repository.Head.Tip;
            }

            var branch = _repository.Branches[name];
            if (branch != null && branch.Tip != null)
            {
                referenceName = branch.FriendlyName;
                return branch.Tip;
            }

            var tag = _repository.Tags[name];
            if (tag == null)
            {
                return _repository.Lookup(name) as Commit;
            }

            referenceName = tag.FriendlyName;
            return tag.Target as Commit;
        }

        private RepositoryCommitModel ToModel(Commit commit, bool withDiff = false)//, Tuple<bool, string, string> linkify)
        {
            string tagsString = string.Empty;
            var tags = _repository.Tags.Where(o => o.Target.Sha == commit.Sha).Select(o => o.FriendlyName).ToList();

            var shortMessageDetails = RepositoryCommitModelHelpers.MakeCommitMessage(commit.Message, 50);

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
                Tags = tags,
                Notes = (from n in commit.Notes select new RepositoryCommitNoteModel(n.Message, n.Namespace)).ToList()
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