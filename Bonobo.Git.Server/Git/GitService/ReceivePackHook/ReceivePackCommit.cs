using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public class ReceivePackCommit
    {
        public ReceivePackCommit(string id, string tree, IEnumerable<string> parents, 
            ReceivePackCommitSignature author, ReceivePackCommitSignature committer, string message)
        {
            this.Id = id;
            this.Tree = tree;
            this.Parents = parents;
            this.Author = author;
            this.Committer = committer;
            this.Message = message;
        }

        public string Id { get; private set; }
        public string Tree { get; private set; }
        public IEnumerable<string> Parents { get; private set; }
        public ReceivePackCommitSignature Author { get; private set; }
        public ReceivePackCommitSignature Committer { get; private set; }
        public string Message { get; private set; }
    }
}