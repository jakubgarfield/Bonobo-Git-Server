using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook.Durability
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

        public class ReceivePackRecoveryDirectory
        {
            public ReceivePackRecoveryDirectory(string receivePackRecoveryDirectory)
            {
                this.Value = receivePackRecoveryDirectory;
            }

            public string Value { get; private set; }
        }
    }
}