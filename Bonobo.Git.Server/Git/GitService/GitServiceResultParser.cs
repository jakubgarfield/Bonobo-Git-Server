using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService
{
    public class GitServiceResultParser
    {
        public GitExecutionResult ParseResult(System.IO.Stream outputStream)
        {
            bool hasError = false;
            if (outputStream.Length >= 10)
            {
                var buff5 = new byte[5];

                if (outputStream.Read(buff5, 0, buff5.Length) != buff5.Length)
                {
                    throw new Exception("Unxepected number of bytes read");
                }
                if (outputStream.Read(buff5, 0, buff5.Length) != buff5.Length)
                {
                    throw new Exception("Unxepected number of bytes read");
                }

                var firstChars = Encoding.ASCII.GetString(buff5);
                hasError = firstChars == "error";
            }
            return new GitExecutionResult(hasError);
        }
    }
}