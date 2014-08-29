using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public class ParseReceivePack : IGitService
    {
        private readonly IGitService gitService;
        private readonly IGitRepositoryLocator repoLocator;
        private readonly IHookReceivePack receivePackHandler;

        public ParseReceivePack(IGitService gitService, IGitRepositoryLocator repoLocator, IHookReceivePack receivePackHandler)
        {
            this.gitService = gitService;
            this.repoLocator = repoLocator;
            this.receivePackHandler = receivePackHandler;
        }

        public void ExecuteServiceByName(string repositoryName, string serviceName, ExecutionOptions options, System.IO.Stream inStream, System.IO.Stream outStream)
        {
            ParsedPack receivedPack = null;

            if (serviceName == "receive-pack" && inStream.Length > 0)
            {
                var headers = new List<ParsedPackRefHeader>();

                // ------------------
                // todo: need to add multi-ref parsing, currently on the first ref is found and extracted
                var buff = new byte[1];
                var accum = new LinkedList<byte>();

                while (inStream.Read(buff, 0, 1) > 0)
                {
                    if (buff[0] == 0)
                    {
                        break;
                    }
                    accum.AddLast(buff[0]);
                }
                var firstLine = Encoding.ASCII.GetString(accum.ToArray());
                var firstLineItems = firstLine.Split(' ');

                var fromCommit = firstLineItems[0].Substring(4);
                var toCommit = firstLineItems[1];
                var refName = firstLineItems[2];

                headers.Add(new ParsedPackRefHeader(fromCommit, toCommit, refName));
                // ------------------

                var user = HttpContext.Current.User.Identity.Name;
                receivedPack = new ParsedPack(Guid.NewGuid().ToString("N"), repositoryName, headers, user, DateTime.Now);

                inStream.Seek(0, SeekOrigin.Begin);
            }

            gitService.ExecuteServiceByName(repositoryName, serviceName, options, inStream, outStream);

            // perhaps need to check if execution succeeded before performing next step?
            if(receivedPack != null)
            {
                var gitRepo = new Repository(repoLocator.GetRepositoryDirectoryPath(repositoryName).FullName);

                var commitData = new List<ReceivePackCommits>();

                foreach(var header in receivedPack.Headers)
                {
                    var affectedCommits = gitRepo.Commits.QueryBy(new CommitFilter()
                    {
                        Since = header.ToCommit,
                        Until = header.ToCommit,
                        SortBy = CommitSortStrategies.Topological
                    });

                    commitData.Add(new ReceivePackCommits(header.RefName, affectedCommits));
                }

                receivePackHandler.PackReceived(receivedPack.PackId, receivedPack.RepositoryName, receivedPack.Timestamp, commitData, receivedPack.PushedByUser);
            }
        }
    }
}