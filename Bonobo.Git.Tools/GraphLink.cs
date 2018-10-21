using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Services.Common;

namespace Bonobo.Git.Tools
{
    [DataServiceKey("Id")]
    public class GraphLink
    {
        public string Id { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

    }
}