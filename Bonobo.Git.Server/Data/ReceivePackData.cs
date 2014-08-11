using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data
{
    /// <summary>
    /// Used for persisting ParasedReceivePack to durable storage
    /// </summary>
    public class ReceivePackData
    {
        public string Data { get; set; }
        public string PackId { get; set; }
        public DateTime EntryTimestamp { get; set; }
    }
}