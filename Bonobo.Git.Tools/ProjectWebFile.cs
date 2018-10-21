using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace Bonobo.Git.Tools
{
    public class ProjectWebFile : VirtualFile
    {
        private string filePath;

        public ProjectWebFile(string filePath) : base(filePath)
        {
            this.filePath = filePath;
        }

        public override Stream Open()
        {
            string fileContents = filePath;
            return File.Open(filePath, FileMode.Open);
        }
    }
}
