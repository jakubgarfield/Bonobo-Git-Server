using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService
{
    /// <summary>
    /// Wrapper around git service execution
    /// </summary>
    public interface IGitService
    {
        void ExecuteServiceByName(string correlationId, string repositoryName, string serviceName, ExecutionOptions options, Stream inStream, Stream outStream);
    }
}