using System;
using System.Data.Services.Common;

namespace Bonobo.Git.Graph
{
    [DataServiceKey("Id")]
    public class Commit
    {
        public string Id { get; set; }
        public string ParentIds { get; set; }
        public string Message { get; set; }
        public string CommitterName { get; set; }
        public string CommitterEmail { get; set; }
        public DateTime CommitDate { get; set; }
        public string CommitDateRelative { get; set; }
        public Tree Tree { get; set; }
    }
}
