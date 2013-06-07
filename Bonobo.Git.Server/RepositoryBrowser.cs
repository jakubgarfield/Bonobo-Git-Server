using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bonobo.Git.Server.Models;
using GitSharp;

namespace Bonobo.Git.Server
{
    public class RepositoryBrowser : IDisposable
    {
        private Repository _repository;
        private string _repositoryPath;
        private bool IsDisposed { get; set; }

        public RepositoryBrowser(string repositoryPath)
        {
            _repositoryPath = repositoryPath;
        }

        public IEnumerable<string> GetBranches()
        {
            if (!EnsureRepository())
            {
                return null;
            }
            return _repository.Branches.Select(i => i.Value.Name);
        }

        public Leaf GetLeaf(string name, string path)
        {
            if (!EnsureRepository())
            {
                return null;
            }

            Branch branch = null;
            Tree source = null;
            Commit commit = null;
            if (TryGetBranch(name, out branch))
            {
                if (branch == null)
                {
                    return null;
                }
                source = (branch.Target as Commit).Tree;
            }
            else if (TryGetCommit(name, out commit))
            {
                source = commit.Tree;
            }
            else
            {
                return null;
            }

            return GetTreeNode(source, path) as Leaf;
        }

        public IEnumerable<RepositoryCommitModel> GetCommits(string branch, out string branchName)
        {
            var result = new List<RepositoryCommitModel>();
            branchName = null;
            if (!EnsureRepository())
            {
                return null;
            }

            Branch currentBranch;
            if (!TryGetBranch(branch, out currentBranch))
            {
                return null;
            }

            if (currentBranch == null)
            {
                return result;
            }

            branchName = currentBranch.Name;


            if (currentBranch.CurrentCommit != null)
            {
                result.Add(ConvertToRepositoryCommitModel(currentBranch.CurrentCommit));
                result.AddRange(currentBranch.CurrentCommit.Ancestors.Select(i => ConvertToRepositoryCommitModel(i)));
            }

            return result.OrderByDescending(i => i.Date);
        }

        public RepositoryCommitModel GetCommitDetail(string id)
        {
            if (!EnsureRepository())
            {
                return null;
            }

            Commit commit;
            if (TryGetCommit(id, out commit))
            {
                return ConvertToRepositoryCommitModel(commit);
            }
            return null;
        }

        public IEnumerable<RepositoryTreeDetailModel> Browse(string treeName, string path)
        {
            string b;
            return Browse(treeName, path, out b);
        }

        public IEnumerable<RepositoryTreeDetailModel> Browse(string name, string path, out string branchName)
        {
            branchName = null;
            if (!EnsureRepository())
            {
                return null;
            }

            var result = new List<RepositoryTreeDetailModel>();

            Tree source = null;
            Branch branch = null;
            Commit commit = null;
            if (TryGetBranch(name, out branch))
            {
                if (branch == null)
                {
                    return result;
                }
                branchName = branch.Name;
                source = (branch.Target as Commit).Tree;
            }
            else if (TryGetCommit(name, out commit))
            {
                source = commit.Tree;
            }
            else
            {
                return null;
            }

            AbstractTreeNode treeNode = GetTreeNode(source, path);
            if (treeNode == null)
            {
                return null;
            }

            if (treeNode.IsTree)
            {
                foreach (AbstractTreeNode item in ((Tree)treeNode).Children)
                {
                    result.Add(ConvertToRepositoryDetailModel(item, GetLastCommit(item, branch, commit), name, branch));
                }
            }
            else if (treeNode as Leaf != null)
            {
                var model = ConvertToRepositoryDetailModel(treeNode, GetLastCommit(treeNode, branch, commit), name, branch);
                model.Data = ((Leaf)treeNode).RawData;
                result.Add(model);
            }
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    if (isDisposing)
                    {
                        if (_repository != null)
                        {
                            _repository.Dispose();
                        }
                    }
                }
            }
            finally
            {
                this.IsDisposed = true;
            }
        }

        protected bool EnsureRepository()
        {
            if (_repository != null && _repository.Directory == _repositoryPath)
            {
                return true;
            }

            if (!GitSharp.Repository.IsValid(_repositoryPath, true))
            {
                return false;
            }
            _repository = new Repository(_repositoryPath);
            return true;
        }

        private Commit GetLastCommit(AbstractTreeNode item, Branch branch, Commit commit)
        {
            if (branch != null)
            {
                return item.GetLastCommit(branch);
            }
            else if (commit != null)
            {
                return item.GetLastCommitBefore(commit);
            }

            return null;
        }

        private RepositoryTreeDetailModel ConvertToRepositoryDetailModel(AbstractTreeNode item, Commit lastCommit, string treeName, Branch branch)
        {
            return new RepositoryTreeDetailModel
            {
                Name = item.Name,
                IsTree = item.IsTree,
                CommitDate = lastCommit != null ? new DateTime?(lastCommit.AuthorDate.LocalDateTime) : null,
                CommitMessage = lastCommit != null ? lastCommit.Message : null,
                Author = lastCommit != null ? lastCommit.Author.Name : null,
                Tree = String.IsNullOrEmpty(treeName) ? branch.Name : treeName,
                Path = item.Path,
                Hash = item.Hash,
            };
        }

        private AbstractTreeNode GetTreeNode(Tree source, string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return source;
            }

            var dirs = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            AbstractTreeNode currentTree = source;
            foreach (var item in dirs)
            {
                if (currentTree.IsTree)
                {
                    var result = ((Tree)currentTree).Children.FirstOrDefault(i => ((AbstractTreeNode)i).Name == item);
                    if (result == null)
                    {
                        return null;
                    }
                    else
                    {
                        currentTree = (AbstractTreeNode)result;
                    }
                }
            }

            return currentTree;
        }

        private bool TryGetCommit(string treeName, out Commit commit)
        {
            commit = null;
            try
            {
                var current = new Commit(_repository, treeName);
                if (current.IsCommit)
                {
                    commit = current;
                    return true;
                }
            }
            catch (ArgumentException)
            {
                return false;
            }

            return false;
        }

        private bool TryGetBranch(string branchName, out Branch branch)
        {
            if (string.IsNullOrEmpty(branchName))
            {
                if (_repository.Head.Target == null)
                {
                    // make the first branch as the default branch
                    if (_repository.Branches.Count() > 0)
                    {
                        branch = _repository.Branches.First().Value;
                        return true;
                    }
                }
                else
                {
                    branch = _repository.Branches.Where(i => i.Value.Target.Hash == _repository.Head.Target.Hash).FirstOrDefault().Value;
                    return true;
                }
            }
            else
            {
                branch = new Branch(_repository, branchName);
                if (branch.IsBranch)
                {
                    return true;
                }
            }
            branch = null;
            return false;
        }

        private RepositoryCommitModel ConvertToRepositoryCommitModel(Commit commit)
        {
            return new RepositoryCommitModel
            {
                Author = commit.Author.Name,
                AuthorEmail = commit.Author.EmailAddress,
                Date = commit.AuthorDate.LocalDateTime,
                ID = commit.Hash,
                Message = commit.Message,
                TreeID = commit.Tree.Hash,
                Parents = commit.Parents.Select(i => i.Hash).ToArray(),
                Changes = commit.Changes.Select(i => new RepositoryCommitChangeModel
                {
                    Name = i.Name,
                    Path = i.Path,
                    Type = i.ChangeType,
                }),
            };
        }

    }
}