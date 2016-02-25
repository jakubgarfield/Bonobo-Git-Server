using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonobo.Git.Server.Test
{
    public class MsysgitResources
    {
        public enum Definition
        {
            CloneEmptyRepositoryOutput,
            CloneEmptyRepositoryError,
            PushFilesError,
            PushTagError,
            PullBranchError,
            PullTagError,
            PullRepositoryError,
            CloneRepositoryOutput,
            CloneRepositoryError,
            PushBranchError,
            CloneRepositoryFailRequiresAuthError,
        }


        private readonly Dictionary<Definition, String> _resources;


        public string this[Definition definition]
        {
            get
            {
                return _resources[definition];
            }
        }


        public MsysgitResources(string version)
        {
            _resources = new Dictionary<Definition, string>
            {
                { Definition.CloneEmptyRepositoryOutput, "Cloning into Integration...\r\n" },
                { Definition.CloneEmptyRepositoryError, "warning: You appear to have cloned an empty repository.\r\n" },
                { Definition.PushFilesError, "To {0}\r\n * [new branch]      master -> master\r\n" },
                { Definition.PushTagError, "To {0}\r\n * [new tag]         v1.4 -> v1.4\r\n" },
                { Definition.PullBranchError, "From {0}\r\n * branch            TestBranch -> FETCH_HEAD\r\n" },
                { Definition.PullTagError, "From {0}\r\n * [new branch]      TestBranch -> origin/TestBranch\r\n * [new branch]      master     -> origin/master\r\n * [new tag]         v1.4       -> v1.4\r\n" },
                { Definition.PullRepositoryError, "From {0}\r\n * branch            master     -> FETCH_HEAD\r\n" },
                { Definition.CloneRepositoryOutput, "Cloning into Integration...\r\n" },
                { Definition.CloneRepositoryError,"" },
                { Definition.PushBranchError, "To {0}\r\n * [new branch]      TestBranch -> TestBranch\r\n" },
            };

            if (String.Equals(version, "1.7.8") 
             || String.Equals(version, "1.7.9") 
             || String.Equals(version, "1.8.0") 
             || String.Equals(version, "1.8.1.2") 
             || String.Equals(version, "1.8.3"))
            {
                _resources[Definition.CloneRepositoryOutput] = "Cloning into 'Integration'...\r\n";
                _resources[Definition.CloneRepositoryError] = "";
                _resources[Definition.CloneEmptyRepositoryOutput] = "Cloning into 'Integration'...\r\n";
            }

            if (String.Equals(version, "1.9.5")
             || String.Equals(version, "2.6.1"))
            {
                _resources[Definition.CloneEmptyRepositoryOutput] = "";
                _resources[Definition.CloneEmptyRepositoryError] = "Cloning into 'Integration'...\r\nwarning: You appear to have cloned an empty repository.\r\n";
                _resources[Definition.CloneRepositoryOutput] = "";
                _resources[Definition.CloneRepositoryError] = "Cloning into 'Integration'...\r\n";
                _resources[Definition.PullRepositoryError] = "From {0}\r\n * branch            master     -> FETCH_HEAD\r\n * [new branch]      master     -> origin/master\r\n";
                _resources[Definition.PullTagError] = "From {0}\r\n * [new branch]      TestBranch -> origin/TestBranch\r\n * [new tag]         v1.4       -> v1.4\r\n";
                _resources[Definition.CloneRepositoryFailRequiresAuthError] = "Cloning into 'Integration'...\r\nbash: /dev/tty: No such device or address\r\nerror: failed to execute prompt script (exit code 1)\r\nfatal: could not read Username for '{0}': Invalid argument\r\n";
            }
        }
    }
}
