using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public class ParsedPack
    {
        public ParsedPack (string packId, string repositoryName, IEnumerable<ParsedPackRefHeader> headers, string pushedByUser, DateTime timestamp)
	    {
            this.PackId = packId;
            this.Headers = headers;
            this.PushedByUser = pushedByUser;
            this.Timestamp = timestamp;
            this.RepositoryName = repositoryName;
	    }

        public string PackId { get; private set; }

        public IEnumerable<ParsedPackRefHeader> Headers { get; private set; }

        public string PushedByUser { get; private set; }

        public DateTime Timestamp { get; private set; }

        public string RepositoryName { get; private set; }
    }
}