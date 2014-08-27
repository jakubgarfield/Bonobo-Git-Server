using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public class ReceivePackCommit
    {
        public ReceivePackCommit(string id)
        {
            this.Id = id;
        }
        public string Id { get; private set; }
    }
}