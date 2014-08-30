using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public class ReceivePackCommit
    {
        public ReceivePackCommit(string id, string tree, string parent, 
            string authorName, string authorEmail, DateTime authorTimestamp,
            string committerName, string committerEmail, DateTime committerTimestamp, string message)
        {
            this.Id = id;
            this.Tree = tree;
            this.Parent = parent;
            
            this.AuthorName = authorName;
            this.AuthorEmail = authorEmail;
            this.AuthorTimestamp = authorTimestamp;

            this.CommitterName = committerName;
            this.CommitterEmail = committerEmail;
            this.CommitterTimestamp = committerTimestamp;

            this.Message = message;
        }

        public string Id { get; private set; }

        public string Tree { get; private set; }
        public string Parent { get; private set; }

        public string AuthorName { get; private set; }
        public string AuthorEmail { get; private set; }
        public DateTime AuthorTimestamp { get; private set; }

        public string CommitterName { get; private set; }
        public string CommitterEmail { get; private set; }
        public DateTime CommitterTimestamp { get; private set; }

        public string Message { get; private set; }
    }
}