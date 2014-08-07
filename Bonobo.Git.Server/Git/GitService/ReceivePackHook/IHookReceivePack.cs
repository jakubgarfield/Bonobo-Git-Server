using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    /// <summary>
    /// Implement this interface to receive notifications when a pack is recieved
    /// and perform any relevant pre/post-processing operations.
    /// </summary>
    public interface IHookReceivePack
    {
        void PrePackReceive(ParsedRecievePack receivePack);

        void PostPackReceive(ParsedRecievePack receivePack, IEnumerable<ReceivePackCommits> commitData);
    }
}