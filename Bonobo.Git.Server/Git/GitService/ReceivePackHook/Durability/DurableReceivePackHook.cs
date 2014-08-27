using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Git.GitService.ReceivePackHook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook.Durability
{
    /// <summary>
    /// Provides at least once execution guarantee to PostPackReceive hook method
    /// </summary>
    public class DurableReceivePackHook : IHookReceivePack
    {
        private readonly TimeSpan failedPackWaitTimeBeforeExecution;
        private readonly IHookReceivePack next;
        private readonly IRecoveryFilePathBuilder recoveryFilePathBuilder;
        private readonly GitServiceResultParser resultFileParser;

        public DurableReceivePackHook(
            IHookReceivePack next, 
            NamedArguments.FailedPackWaitTimeBeforeExecution failedPackWaitTimeBeforeExecution,
            IRecoveryFilePathBuilder recoveryFilePathBuilder,
            GitServiceResultParser resultFileParser)
        {
            this.next = next;
            this.failedPackWaitTimeBeforeExecution = failedPackWaitTimeBeforeExecution.Value;
            this.recoveryFilePathBuilder = recoveryFilePathBuilder;
            this.resultFileParser = resultFileParser;
        }

        public void PrePackReceive(ParsedReceivePack receivePack)
        {
            File.WriteAllText(recoveryFilePathBuilder.GetPathToPackFile(receivePack), JsonConvert.SerializeObject(receivePack));
            next.PrePackReceive(receivePack);
        }

        public void PostPackReceive(ParsedReceivePack receivePack, GitExecutionResult result)
        {
            var waitingReceivePacks = new List<ParsedReceivePack>();

            foreach(var packDir in recoveryFilePathBuilder.GetPathToPackDirectory())
            {
                foreach (var packFilePath in Directory.GetFiles(packDir))
                {
                    using(var fileReader = new StreamReader(packFilePath))
                    {
                        var packFileData = fileReader.ReadToEnd();
                        waitingReceivePacks.Add(JsonConvert.DeserializeObject<ParsedReceivePack>(packFileData));
                    }
                }
            }

            foreach (var pack in waitingReceivePacks.OrderBy(p => p.Timestamp))
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
                    var failedPackResultFilePath = recoveryFilePathBuilder.GetPathToResultFile(receivePack.PackId, receivePack.RepositoryName, "receive-pack");
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
                var packFilePath = recoveryFilePathBuilder.GetPathToPackFile(pack);
                if (File.Exists(packFilePath))
                {
                    File.Delete(packFilePath);
                }
            }
        }
    }
}