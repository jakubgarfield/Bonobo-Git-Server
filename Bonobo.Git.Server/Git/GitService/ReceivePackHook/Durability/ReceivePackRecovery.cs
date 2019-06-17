using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook.Durability
{
    /// <summary>
    /// Provides at least once execution guarantee to PostPackReceive hook method
    /// </summary>
    public class ReceivePackRecovery : IHookReceivePack
    {
        private readonly IHookReceivePack _next;
        private readonly IRecoveryFilePathBuilder _recoveryFilePathBuilder;
        private readonly GitServiceResultParser _resultFileParser;

        public ReceivePackRecovery(
            IHookReceivePack next, 
            IRecoveryFilePathBuilder recoveryFilePathBuilder,
            GitServiceResultParser resultFileParser)
        {
            _next = next;
            _recoveryFilePathBuilder = recoveryFilePathBuilder;
            _resultFileParser = resultFileParser;
        }

        public void PrePackReceive(ParsedReceivePack receivePack)
        {
            File.WriteAllText(_recoveryFilePathBuilder.GetPathToPackFile(receivePack), JsonConvert.SerializeObject(receivePack));
            _next.PrePackReceive(receivePack);
        }

        public void PostPackReceive(ParsedReceivePack receivePack, GitExecutionResult result)
        {
            ProcessOnePack(receivePack, result);
            RecoverAll();
        }

        private void ProcessOnePack(ParsedReceivePack receivePack, GitExecutionResult result)
        {
            _next.PostPackReceive(receivePack, result);
            
            var packFilePath = _recoveryFilePathBuilder.GetPathToPackFile(receivePack);
            if (File.Exists(packFilePath))
            {
                File.Delete(packFilePath);
            }
        }

        public void RecoverAll()
        {
            RecoverAll(TimeSpan.FromSeconds(5 * 60));
        }

        public void RecoverAll(TimeSpan failedPackWaitTimeBeforeExecution)
        {
            var waitingReceivePacks = new List<ParsedReceivePack>();

            foreach (var packDir in _recoveryFilePathBuilder.GetPathToPackDirectory())
            {
                foreach (var packFilePath in Directory.GetFiles(packDir))
                {
                    using (var fileReader = new StreamReader(packFilePath))
                    {
                        var packFileData = fileReader.ReadToEnd();
                        waitingReceivePacks.Add(JsonConvert.DeserializeObject<ParsedReceivePack>(packFileData));
                    }
                }
            }

            foreach (var pack in waitingReceivePacks.OrderBy(p => p.Timestamp))
            {
                // execute if the pack has been waiting for X amount of time
                if ((DateTime.Now - pack.Timestamp) >= failedPackWaitTimeBeforeExecution)
                {
                    // re-parse result file and execute "post" hooks
                    // if result file is no longer there then move on
                    var failedPackResultFilePath = _recoveryFilePathBuilder.GetPathToResultFile(pack.PackId, pack.RepositoryName, "receive-pack");
                    if (File.Exists(failedPackResultFilePath))
                    {
                        using (var resultFileStream = File.OpenRead(failedPackResultFilePath))
                        {
                            var failedPackResult = _resultFileParser.ParseResult(resultFileStream);
                            ProcessOnePack(pack, failedPackResult);
                        }
                        File.Delete(failedPackResultFilePath);
                    }
                }
            }
        }
    }
}