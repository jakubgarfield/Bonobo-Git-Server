using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public class ReceivePackCommitSignature
    {
        public ReceivePackCommitSignature(string name, string email, DateTimeOffset timestamp)
        {
            this.Name = name;
            this.Email = email;
            this.Timestamp = timestamp;
        }

        public string Name { get; private set; }

        public string Email { get; private set; }

        public DateTimeOffset Timestamp { get; private set; }
    }
}