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
            CloneRepositoryError,
            PushBranchError,
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
                { Definition.CloneEmptyRepositoryOutput, "Cloning into Integration...\n" },
                { Definition.CloneEmptyRepositoryError, "warning: You appear to have cloned an empty repository.\n" },
                { Definition.PushFilesError, "To {0}\n * [new branch]      master -> master\n" },
                { Definition.PushTagError, "To {0}\n * [new tag]         v1.4 -> v1.4\n" },
                { Definition.PullBranchError, "From {0}\n * branch            TestBranch -> FETCH_HEAD\n" },
                { Definition.PullTagError, "From {0}\n * [new branch]      TestBranch -> origin/TestBranch\n * [new branch]      master     -> origin/master\n * [new tag]         v1.4       -> v1.4\n" },
                { Definition.PullRepositoryError, "From {0}\n * branch            master     -> FETCH_HEAD\n" },
                { Definition.CloneRepositoryError, "Cloning into Integration...\n" },
                { Definition.PushBranchError, "To {0}\n * [new branch]      TestBranch -> TestBranch\n" },
            };

            if (String.Equals(version, "1.7.8") 
             || String.Equals(version, "1.7.9") 
             || String.Equals(version, "1.8.0") 
             || String.Equals(version, "1.8.1.2") 
             || String.Equals(version, "1.8.3"))
            {
                _resources[Definition.CloneRepositoryError] = "Cloning into 'Integration'...\n";
                _resources[Definition.CloneEmptyRepositoryOutput] = "Cloning into 'Integration'...\n";
            }
        }
    }
}
