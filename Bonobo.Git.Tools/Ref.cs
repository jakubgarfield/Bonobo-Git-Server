using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Services.Common;

namespace Bonobo.Git.Tools
{
    [DataServiceKey("Id")]
    public class Ref
    {
        public string Id { get; set; }
        public string RefName { get; set; }
        public string Name
        {
            get
            {
                return RefName.Substring(RefName.IndexOf("/") + 1);
            }
        }
        public string Type
        {
            get
            {
                return RefName.Substring(0, RefName.IndexOf("/"));
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}]", Name);
        }
    }
}