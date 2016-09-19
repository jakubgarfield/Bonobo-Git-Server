using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook.Durability
{
    /// <summary>
    /// Ensures directories for generated recovery paths exist
    /// </summary>
    public class AutoCreateMissingRecoveryDirectories : IRecoveryFilePathBuilder
    {
        private readonly IRecoveryFilePathBuilder pathBuilder;

        public AutoCreateMissingRecoveryDirectories(IRecoveryFilePathBuilder pathBuilder)
        {
            this.pathBuilder = pathBuilder; 
        }

        public string CreateDirectoryForFile(string filePath)
        {
            var dirPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(dirPath);
            return filePath;
        }

        public string GetPathToResultFile(string correlationId, string repositoryName, string serviceName)
        {
            return CreateDirectoryForFile(pathBuilder.GetPathToResultFile(correlationId, repositoryName, serviceName));
        }

        public string GetPathToPackFile(ParsedReceivePack receivePack)
        {
            return CreateDirectoryForFile(pathBuilder.GetPathToPackFile(receivePack));
        }

        public string[] GetPathToPackDirectory()
        {
            var dirs = pathBuilder.GetPathToPackDirectory();
            foreach(var dir in dirs)
            {
                Directory.CreateDirectory(dir);
            }
            return dirs;
        }
    }
}