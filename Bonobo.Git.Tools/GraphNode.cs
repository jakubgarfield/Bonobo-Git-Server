using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Services.Common;

namespace Bonobo.Git.Tools
{
    [DataServiceKey("Id")]
    public class GraphNode
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string CommitterName { get; set; }
        public string CommitDateRelative { get; set; }
        public string Tags { get; set; }
        public string Branches { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}