using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.IntegrationTests
{
    public class GitResult
    {
        public string StdErr { get; set; }
        public string StdOut { get; set; }
        public int ExitCode { get; set; }
        public MsysgitResources Resources { get; set; }

        public bool Succeeded
        {
            get { return ExitCode == 0; }
        }

        public bool AccessDenied
        {
            get { return !Succeeded && StdErr.Contains(Resources[MsysgitResources.Definition.AuthenticationFailedError]); }
        }

        public void ExpectSuccess()
        {
            if (!Succeeded)
            {
                Assert.Fail("Git operation failed with exit code {0}, stderr {1}", ExitCode, StdErr);
            }
        }

        public void ErrorMustMatch(MsysgitResources.Definition resource, params object[] args)
        {
            string matchString;
            if (args.Length > 0)
            {
                matchString = string.Format(Resources[resource], args);
            }
            else
            {
                matchString = Resources[resource];
            }
            var expected = matchString.Trim();
            var actual = StdErr.Trim();
            if (expected != actual)
            {
                Assert.Fail("Git operation StdErr mismatch - expected '{0}', was '{1}'", expected, actual);
            }
        }
    }
}