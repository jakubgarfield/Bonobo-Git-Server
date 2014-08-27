using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.Durability
{
    /// <summary>
    /// Provides at least once execution guarantee to PostPackReceive hook method
    /// </summary>
    public class DurableReceivePackHook : IHookReceivePack
    {
        private readonly TimeSpan failedPackWaitTimeBeforeExecution;
        private readonly IHookReceivePack next;
        private readonly IReceivePackRepository receivePackRepo;
        private readonly IResultFilePathBuilder resultFilePathBuilder;
        private readonly GitServiceResultParser resultFileParser;

        public DurableReceivePackHook(
            IHookReceivePack next, 
            IReceivePackRepository receivePackRepo, 
            NamedArguments.FailedPackWaitTimeBeforeExecution failedPackWaitTimeBeforeExecution,
            IResultFilePathBuilder resultFilePathBuilder,
            GitServiceResultParser resultFileParser)
        {
            this.next = next;
            this.receivePackRepo = receivePackRepo;
            this.failedPackWaitTimeBeforeExecution = failedPackWaitTimeBeforeExecution.Value;
            this.resultFilePathBuilder = resultFilePathBuilder;
            this.resultFileParser = resultFileParser;
        }

        public void PrePackReceive(ParsedReceivePack receivePack)
        {
            receivePackRepo.Add(receivePack);
            next.PrePackReceive(receivePack);
        }

        public void PostPackReceive(ParsedReceivePack receivePack, GitExecutionResult result)
        {
            foreach (var pack in receivePackRepo.All().OrderBy(p => p.Timestamp))
            {
                // execute PostPackReceive right away only for the current pack
                // or if the pack has been sitting in database for X amount of time
                var isFailedPack = ((DateTime.Now - pack.Timestamp) >= failedPackWaitTimeBeforeExecution);

                if (pack.PackId == receivePack.PackId)
                {
                    next.PostPackReceive(pack, result);
                }
                else if (isFailedPack)
                {
                    // for failed pack re-parse result file and execute "post" hooks
                    // if result file is no longer there then move on
                    var failedPackResultFilePath = resultFilePathBuilder.GetPathToResultFile(receivePack.PackId, receivePack.RepositoryName, "receive-pack");
                    if (File.Exists(failedPackResultFilePath))
                    {
                        using (var resultFileStream = File.OpenRead(failedPackResultFilePath))
                        {
                            var failedPackResult = resultFileParser.ParseResult(resultFileStream);
                            next.PostPackReceive(receivePack, failedPackResult);
                        }
                        File.Delete(failedPackResultFilePath);
                    }
                }
                receivePackRepo.Delete(pack.PackId);
            }
        }
    }
}