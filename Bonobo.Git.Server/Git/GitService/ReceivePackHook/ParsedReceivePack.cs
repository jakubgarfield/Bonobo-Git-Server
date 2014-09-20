using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public class ParsedReceivePack
    {
        public ParsedReceivePack (string packId, string repositoryName, IEnumerable<ReceivePackPktLine> pktLines, string pushedByUser, DateTime timestamp, IEnumerable<ReceivePackCommit> commits)
	    {
            this.PackId = packId;
            this.PktLines = pktLines;
            this.PushedByUser = pushedByUser;
            this.Timestamp = timestamp;
            this.RepositoryName = repositoryName;
            this.Commits = commits;
	    }

        public string PackId { get; private set; }

        public IEnumerable<ReceivePackPktLine> PktLines { get; private set; }

        public IEnumerable<ReceivePackCommit> Commits { get; private set; }

        public string PushedByUser { get; private set; }

        public DateTime Timestamp { get; private set; }

        public string RepositoryName { get; private set; }
    }
}