using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Services.Common;
using System.IO;
using System.Text;

namespace Bonobo.Git.Tools
{
    [DataServiceKey("Id")]
    public class BlobContent
    {
        public string Id { get; set; }
        public string RepoFolder { get; set; }

        private byte[] bytes;
        public byte[] Bytes
        {
            get
            {
                if (bytes == null)
                {
                    var fileName = Path.GetTempFileName();

                    Git.RunCmd("cat-file -p " + this.Id + " > " + fileName,
                        this.RepoFolder);
                    
                    bytes = File.ReadAllBytes(fileName);

                    if (File.Exists(fileName)) File.Delete(fileName);
                }
                return bytes;
            }
        }

    }
}