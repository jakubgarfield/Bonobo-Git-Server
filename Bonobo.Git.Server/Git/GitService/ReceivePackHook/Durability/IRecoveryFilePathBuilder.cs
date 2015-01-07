using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook.Durability
{
    public interface IRecoveryFilePathBuilder
    {
        string GetPathToResultFile(string correlationId, string repositoryName, string serviceName);

        string GetPathToPackFile(ParsedReceivePack receivePack);

        string[] GetPathToPackDirectory();
    }
}