using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.Durability
{
    /// <summary>
    /// Generates paths all going into one configured folder
    /// </summary>
    public class OneFolderResultFilePathBuilder : IResultFilePathBuilder
    {
        private static Regex illegalChars = new Regex("([/\\:*?\"<>|])");
        private readonly string receivePackFileResultDirectory;

        public OneFolderResultFilePathBuilder(NamedArguments.ReceivePackFileResultDirectory receivePackFileResultDirectory)
        {
            this.receivePackFileResultDirectory = receivePackFileResultDirectory.Value;
        }

        public string GetPathToResultFile(string correlationId, string repositoryName, string serviceName)
        {
            var path = string.Format("{0}-{1}-{2}.result", repositoryName, serviceName, correlationId);

            return Path.Combine(receivePackFileResultDirectory, illegalChars.Replace(path, ""));
        }
    }
}