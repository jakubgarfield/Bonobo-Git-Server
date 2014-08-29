using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    /// <summary>
    /// Implement this interface to receive notifications when a pack is recieved
    /// and perform any relevant post-processing operations.
    /// </summary>
    public interface IHookReceivePack
    {
        void PackReceived(string packId, string repositoryName, DateTime timestamp, IEnumerable<ReceivePackCommits> commitData, string pushedByUser);
    }
}