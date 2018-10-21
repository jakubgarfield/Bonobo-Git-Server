using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;

namespace Bonobo.Git.Tools
{
    [DataServiceKey("Id")]
    public class Repository
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RepoFolder { get; set; }

        public static Repository Open(string directory)
        {
            var repo = new Repository
            {
                Name = Path.GetFileNameWithoutExtension(directory),
                RepoFolder = directory,
                Id = GetId(directory)
            };
            return repo;
        }

        private static string GetId(string directory)
        {
            var baseFolder = ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"];
            return directory.Substring(baseFolder.Length + 1).Replace("\\", ".").Replace(".git", "");
        }

        public static bool IsValid(string path)
        {
            if (path == null)
                return false;
            if (!Directory.Exists(path))
                return false;
            if (!File.Exists(Path.Combine(path, "HEAD")))
                return false;
            if (!File.Exists(Path.Combine(path, "config")))
                return false;
            if (!Directory.Exists(Path.Combine(path, "objects")))
                return false;
            if (!Directory.Exists(Path.Combine(path, "objects/info")))
                return false;
            if (!Directory.Exists(Path.Combine(path, "objects/pack")))
                return false;
            if (!Directory.Exists(Path.Combine(path, "refs")))
                return false;
            if (!Directory.Exists(Path.Combine(path, "refs/heads")))
                return false;
            if (!Directory.Exists(Path.Combine(path, "refs/tags")))
                return false;
            return true;
        }
        
        //public IEnumerable<Branch> Branches
        //{
        //    get
        //    {
        //        var branches = from b in Git.Run("branch", this.RepoFolder).Split('\n')
        //                       where !string.IsNullOrWhiteSpace(b)
        //                       select new Branch { Name = b.Substring(2) };
        //        return branches;
        //    }
        //}

        //public string CurrentBranch
        //{
        //    get
        //    {
        //        var branches = from b in Git.Run("branch", this.RepoFolder).Split('\n')
        //                       where b.StartsWith("*")
        //                       select b.Substring(2);
        //        return branches.FirstOrDefault();
        //    }
        //}

        public IEnumerable<Commit> Commits
        {
            get
            {
                var output = "";
                try
                {
                    output = Git.Run("log -n 100 --date-order HEAD --pretty=format:%H`%P`%cr`%cn`%ce`%ci`%T`%s --all --boundary", this.RepoFolder);
                }
                catch
                {
                    
                }
                if (!string.IsNullOrEmpty(output))
                {
                    var logs = output.Split('\n');
                    foreach (string log in logs)
                    {
                        string[] ss = log.Split('`');

                        if (ss[0].Contains("'")) ss[0] = ss[0].Replace("'", "");

                        yield return new Commit
                        {
                            Id = ss[0],
                            ParentIds = ss[1],
                            CommitDateRelative = ss[2],
                            CommitterName = ss[3],
                            CommitterEmail = ss[4],
                            CommitDate = DateTime.Parse(ss[5]),
                            Tree = new Tree
                            {
                                Id = ss[6],
                                RepoFolder = this.RepoFolder,
                                Name = "",
                            },
                            Message = ss[7] + (ss.Length <= 8 ? "" : "`" + string.Join("`", ss, 8, ss.Length - 8))
                        };
                    }
                }
            }
        }

        public IEnumerable<Ref> Refs
        {
            get
            {
                var refs = from t in Git.Run("show-ref", this.RepoFolder).Split('\n')
                           where !string.IsNullOrWhiteSpace(t)
                           select new Ref 
                           { 
                               Id = t.Substring(0, 40),
                               RefName = t.Substring(46)
                           };
                return refs;
            }
        }
    }
}