using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Services.Common;
using System.Text;

namespace Bonobo.Git.Tools
{
    [DataServiceKey("Id")]
    public class Blob
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public BlobContent Content { get; set; }
    }
}