using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.Durability
{
    /// <summary>
    /// Perhaps there's a better way to handle wiring up simple types in Unity but i haven't found it
    /// </summary>
    public class NamedArguments
    {
        public class FailedPackWaitTimeBeforeExecution
        {
            public FailedPackWaitTimeBeforeExecution(TimeSpan timeSpan)
            {
                this.Value = timeSpan;
            }
            public TimeSpan Value { get; private set; }
        }

        public class ReceivePackFileResultDirectory
        {
            public ReceivePackFileResultDirectory(string receivePackFileResultDirectory)
            {
                this.Value = receivePackFileResultDirectory;
            }

            public string Value { get; private set; }
        }
    }
}