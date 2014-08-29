using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook.Hooks
{
    public class NullReceivePackHook : IHookReceivePack
    {
        public void PrePackReceive(ParsedRecievePack receivePack)
        {
            // do nothing
        }

        public void PostPackReceive(ParsedRecievePack receivePack, IEnumerable<ReceivePackCommits> commitData)
        {
            // do nothing
        }
    }
}