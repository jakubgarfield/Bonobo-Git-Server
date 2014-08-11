using Bonobo.Git.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook.Hooks
{
    /// <summary>
    /// Provides at least once execution guarantee to PostPackReceive hook method
    /// </summary>
    public class DurableReceivePackHook : IHookReceivePack
    {
        private readonly int failedPackWaitTimeBeforeExecutionInSec;
        private readonly IHookReceivePack next;
        private readonly IReceivePackRepository receivePackRepo;

        // TODO: change failedPackWaitTimeBeforeExecutionInSec to be an injection param
        public DurableReceivePackHook(IHookReceivePack next, IReceivePackRepository receivePackRepo) // int failedPackWaitTimeBeforeExecutionInSec)
        {
            this.next = next;
            this.receivePackRepo = receivePackRepo;
            this.failedPackWaitTimeBeforeExecutionInSec = 5 * 60; //failedPackWaitTimeBeforeExecutionInSec;
        }

        public void PrePackReceive(ParsedReceivePack receivePack)
        {
            receivePackRepo.Add(receivePack);
            next.PrePackReceive(receivePack);
        }

        public void PostPackReceive(ParsedReceivePack receivePack)
        {
            foreach (var pack in receivePackRepo.All().OrderBy(p => p.Timestamp))
            {
                // execute PostPackReceive right away only for the current pack
                // or if the pack has been sitting in database for X amount of time
                if (pack.PackId == receivePack.PackId
                    || (DateTime.Now - pack.Timestamp).TotalSeconds >= failedPackWaitTimeBeforeExecutionInSec)
                {
                    next.PostPackReceive(pack);
                    receivePackRepo.Delete(pack.PackId);
                }
            }
        }
    }
}