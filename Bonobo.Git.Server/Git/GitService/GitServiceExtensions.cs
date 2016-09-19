using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService
{
    public static class GitServiceExtensions
    {
        public static void ExecuteGitUploadPack(this IGitService self, string correlationId, string repositoryName, Stream inStream, Stream outStream)
        {
            self.ExecuteServiceByName(
                correlationId,
                repositoryName,
                "upload-pack",
                new ExecutionOptions() { AdvertiseRefs = false, endStreamWithClose = true },
                inStream,
                outStream);
        }

        public static void ExecuteGitReceivePack(this IGitService self, string correlationId, string repositoryName, Stream inStream, Stream outStream)
        {
            self.ExecuteServiceByName(
                correlationId,
                repositoryName,
                "receive-pack",
                new ExecutionOptions() { AdvertiseRefs = false },
                inStream,
                outStream);
        }
    }
}