using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public class ReceivePackPktLine
    {
        public ReceivePackPktLine(string fromCommit, string toCommit, string refName)
        {
            this.FromCommit = fromCommit;
            this.ToCommit = toCommit;
            this.RefName = refName;
        }
        public string FromCommit { get; private set; }
        public string ToCommit { get; private set; }
        public string RefName { get; private set; }
    }
}