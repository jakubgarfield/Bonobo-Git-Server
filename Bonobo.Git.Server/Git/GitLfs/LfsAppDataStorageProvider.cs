using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitLfs
{
    /// <summary>
    /// This is a terrible implementation.  Let's just get it working first.
    /// </summary>
    public class LfsAppDataStorageProvider : ILfsDataStorageProvider
    {
        public string GetFileUrl(string urlScheme, string urlAuthority, string requestApplicationPath, string operation, string repositoryName, string oid, long size)
        {
            string url = string.Concat(
                urlScheme,
                "://",
                urlAuthority,
                "/",
                repositoryName,
                ".git",
                requestApplicationPath,
                "lfs/oid/",
                oid);
            return url;
        }

        private string DetermineAppDataPath()
        {
            string candidate1 = HttpContext.Current.Server.MapPath("~/App_Data");
            if (System.IO.Directory.Exists(candidate1))
                return candidate1;

            string candidate2 = HttpContext.Current.Server.MapPath("~/bin/App_Data");
            if (System.IO.Directory.Exists(candidate2))
                return candidate2;

            throw new DirectoryNotFoundException("Unable to determine path to App_Data.");
        }
        
        private string DetermineFilename(string repositoryName, string oid)
        {
            string appDataPath = DetermineAppDataPath();
            string firstTwo = (oid + "xx").Substring(0, 2).ToLower();
            string secondTwo = (oid + "xxyy").Substring(2, 2).ToLower();
            var filename = Path.Combine(appDataPath, "Lfs", repositoryName, firstTwo, secondTwo, oid);
            return filename;
        }

        public Stream GetWriteStream(string operation, string repositoryName, string oid)
        {

            string filename = DetermineFilename(repositoryName, oid);
            string directoryName = System.IO.Path.GetDirectoryName(filename);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            return new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        }

        public Stream GetReadStream(string operation, string repositoryName, string oid)
        {
            string filename = DetermineFilename(repositoryName, oid);

            if (System.IO.File.Exists(filename))
                return new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            else
                throw new FileNotFoundException("File not found.", filename);
        }

        public bool Exists(string repositoryName, string oid)
        {
            string filename = DetermineFilename(repositoryName, oid);

            return  System.IO.File.Exists(filename);
        }

        public bool SufficientSpace(long requiredSpace)
        {
            var path = DetermineAppDataPath();
            var di = new DriveInfo(path);
            return di.AvailableFreeSpace > requiredSpace;
        }
    }
}