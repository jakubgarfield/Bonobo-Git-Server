using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.Durability
{
    public interface IResultFilePathBuilder
    {
        string GetPathToResultFile(string correlationId, string repositoryName, string serviceName);
    }
}