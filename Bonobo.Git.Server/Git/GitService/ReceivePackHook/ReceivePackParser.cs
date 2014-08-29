using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public class ReceivePackParser : IGitService
    {
        private readonly IGitService gitService;
        private readonly IGitRepositoryLocator repoLocator;
        private readonly IHookReceivePack receivePackHandler;

        public ReceivePackParser(IGitService gitService, IGitRepositoryLocator repoLocator, IHookReceivePack receivePackHandler)
        {
            this.gitService = gitService;
            this.repoLocator = repoLocator;
            this.receivePackHandler = receivePackHandler;
        }

        public void ExecuteServiceByName(string repositoryName, string serviceName, ExecutionOptions options, System.IO.Stream inStream, System.IO.Stream outStream)
        {
            ParsedRecievePack receivedPack = null;

            if (serviceName == "receive-pack" && inStream.Length > 0)
            {
                //string content = new StreamReader(inStream).ReadToEnd();
                //inStream.Seek(0, SeekOrigin.Begin);

                // PARSING RECEIVE-PACK THAT IS OF THE FOLLOWING FORMAT: 
                // (NEW LINES added for ease of reading)
                // (LLLL is length of the line (expressed in HEX) until next LLLL value)
                //
                // LLLL------ REF LINE -----------\0------- OHTER DATA -----------
                // LLLL------ REF LINE ----------------
                // ...
                // ...
                // 0000PACK------- REST OF PACKAGE --------
                //


                var refChanges = new List<ReceivePackRefChange>();

                while (true)
                {
                    var lenBytes = new byte[4];
                    if (inStream.Read(lenBytes, 0, 4) != 4)
                    {
                        throw new Exception("Unexpected receive-pack 'length' content.");
                    }
                    var len = Convert.ToInt32(Encoding.ASCII.GetString(lenBytes), 16);
                    if (len == 0)
                    {
                        break;
                    }
                    len = len - lenBytes.Length;

                    var buff = new byte[1];
                    var accum = new LinkedList<byte>();

                    while (len > 0)
                    {
                        len -= 1;
                        if (inStream.Read(buff, 0, 1) != 1)
                        {
                            throw new Exception("Unexpected receive-pack 'header' content.");
                        }
                        if (buff[0] == 0)
                        {
                            break;
                        }
                        accum.AddLast(buff[0]);
                    }
                    if (len > 0)
                    {
                        inStream.Seek(len, SeekOrigin.Current);
                    }
                    var refLine = Encoding.ASCII.GetString(accum.ToArray());
                    var refLineItems = refLine.Split(' ');

                    var fromCommit = refLineItems[0];
                    var toCommit = refLineItems[1];
                    var refName = refLineItems[2];

                    refChanges.Add(new ReceivePackRefChange(fromCommit, toCommit, refName));
                }

                var user = HttpContext.Current.User.Identity.Name;
                receivedPack = new ParsedRecievePack(Guid.NewGuid().ToString("N"), repositoryName, refChanges, user, DateTime.Now);

                inStream.Seek(0, SeekOrigin.Begin);

                receivePackHandler.PrePackReceive(receivedPack);
            }

            gitService.ExecuteServiceByName(repositoryName, serviceName, options, inStream, outStream);

            // perhaps need to check if execution succeeded before performing next step?
            if(receivedPack != null)
            {
                var gitRepo = new Repository(repoLocator.GetRepositoryDirectoryPath(repositoryName).FullName);

                var commitData = new List<ReceivePackCommits>();

                foreach(var refChange in receivedPack.RefChanges)
                {
                    var affectedCommits = gitRepo.Commits.QueryBy(new CommitFilter()
                    {
                        Since = refChange.ToCommit,
                        Until = refChange.FromCommit,
                        SortBy = CommitSortStrategies.Topological
                    });

                    commitData.Add(new ReceivePackCommits(refChange.RefName, affectedCommits));
                }

                receivePackHandler.PostPackReceive(receivedPack, commitData);
            }
        }
    }
}