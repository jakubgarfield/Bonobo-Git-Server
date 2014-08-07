using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService
{
    public static class GitServiceExtensions
    {
        public static void ExecuteGitUploadPack(this IGitService self, string repositoryName, Stream inStream, Stream outStream)
        {
            self.ExecuteServiceByName(
                repositoryName,
                "upload-pack",
                new ExecutionOptions() { AdvertiseRefs = false },
                inStream,
                outStream);
        }

        public static void ExecuteGitReceivePack(this IGitService self, string repositoryName, Stream inStream, Stream outStream)
        {
            self.ExecuteServiceByName(
                repositoryName,
                "receive-pack",
                new ExecutionOptions() { AdvertiseRefs = false },
                inStream,
                outStream);
        }
    }
}