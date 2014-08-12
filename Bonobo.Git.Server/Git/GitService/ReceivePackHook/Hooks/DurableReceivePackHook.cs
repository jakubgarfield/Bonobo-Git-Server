using Bonobo.Git.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook.Hooks
{
    /// <summary>
    /// Perhaps there's a better way to handle this in Unity but i haven't found it
    /// </summary>
    public class FailedPackWaitTimeBeforeExecution
    {
        public FailedPackWaitTimeBeforeExecution(TimeSpan timeSpan)
        {
            this.Value = timeSpan;
        }
        public TimeSpan Value { get; private set; }
    }

    /// <summary>
    /// Provides at least once execution guarantee to PostPackReceive hook method
    /// </summary>
    public class DurableReceivePackHook : IHookReceivePack
    {
        private readonly TimeSpan failedPackWaitTimeBeforeExecution;
        private readonly IHookReceivePack next;
        private readonly IReceivePackRepository receivePackRepo;

        public DurableReceivePackHook(IHookReceivePack next, IReceivePackRepository receivePackRepo, FailedPackWaitTimeBeforeExecution failedPackWaitTimeBeforeExecution)
        {
            this.next = next;
            this.receivePackRepo = receivePackRepo;
            this.failedPackWaitTimeBeforeExecution = failedPackWaitTimeBeforeExecution.Value;
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
                    || (DateTime.Now - pack.Timestamp) >= failedPackWaitTimeBeforeExecution)
                {
                    next.PostPackReceive(pack);
                    receivePackRepo.Delete(pack.PackId);
                }
            }
        }
    }
}