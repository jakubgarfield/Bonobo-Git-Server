using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook.Durability
{
    /// <summary>
    /// Generates paths all going into one configured folder
    /// </summary>
    public class OneFolderRecoveryFilePathBuilder : IRecoveryFilePathBuilder
    {
        private static Regex illegalChars = new Regex("([/\\:*?\"<>|])");
        private readonly string receivePackRecoveryDirectory;

        public OneFolderRecoveryFilePathBuilder(NamedArguments.ReceivePackRecoveryDirectory receivePackRecoveryDirectory)
        {
            this.receivePackRecoveryDirectory = receivePackRecoveryDirectory.Value;
        }

        public string StripIllegalChars(string input)
        {
            return illegalChars.Replace(input, "");
        }

        public string GetPathToResultFile(string correlationId, string repositoryName, string serviceName)
        {
            var path = string.Format("{0}.{1}.{2}.result", repositoryName, serviceName, correlationId);

            return Path.Combine(receivePackRecoveryDirectory, StripIllegalChars(path));
        }


        public string GetPathToPackFile(ParsedReceivePack receivePack)
        {
            return Path.Combine(
                        receivePackRecoveryDirectory,
                        "ReceivePack",
                        StripIllegalChars(string.Format("{0}.{1}.pack", receivePack.RepositoryName, receivePack.PackId)));
        }


        public string[] GetPathToPackDirectory()
        {
            return new string [] { Path.Combine(receivePackRecoveryDirectory, "ReceivePack") };
        }
    }
}