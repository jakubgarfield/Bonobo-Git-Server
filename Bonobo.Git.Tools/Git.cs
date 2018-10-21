using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Configuration;
using System.IO;

namespace Bonobo.Git.Tools
{
    public abstract class Git
    {
        private const string TRACE_CATEGORY = "git";
        public const string GIT_EXTENSION = "git";

        public static string Run(string args, string workingDirectory)
        {
            var GitPath = ConfigurationManager.AppSettings["GitPath"];

            Trace.WriteLine(string.Format("{2}>{0} {1}", GitPath, args, workingDirectory), TRACE_CATEGORY);

            var pinfo = new ProcessStartInfo(GitPath)
            {
                Arguments = args,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory,
            };

            using (var process = Process.Start(pinfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Trace.WriteLine(output, TRACE_CATEGORY);

                if (!string.IsNullOrEmpty(error))
                {
                    Trace.WriteLine("STDERR: " + error, TRACE_CATEGORY);
                    throw new Exception(error);
                }
                return output;
            }
        }


        public static void RunCmd(string args, string workingDirectory)
        {

            var GitPath = ConfigurationManager.AppSettings["GitPath"];

            Trace.WriteLine(string.Format("{2}>{0} {1}", GitPath, args, workingDirectory), TRACE_CATEGORY);

            var pinfo = new ProcessStartInfo("cmd.exe")
            {
                Arguments = "/C \"\"" + GitPath + "\"\" " + args,
                CreateNoWindow = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory,
            };

            using (var process = Process.Start(pinfo))
            {
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                    throw new Exception(error);
            }
        }

        //public static void Run(string args, string workingDirectory, Action<string> action)
        //{
        //    var GitPath = ConfigurationManager.AppSettings["GitPath"];

        //    Trace.WriteLine(string.Format("{2}>{0} {1}", GitPath, args, workingDirectory), TRACE_CATEGORY);

        //    var pinfo = new ProcessStartInfo(GitPath)
        //    {
        //        Arguments = args,
        //        CreateNoWindow = true,
        //        RedirectStandardError = true,
        //        RedirectStandardOutput = true,
        //        UseShellExecute = false,
        //        WorkingDirectory = workingDirectory,
        //    };

        //    var process = new Process();
        //    process.StartInfo = pinfo;
        //    process.EnableRaisingEvents = true;

        //    process.OutputDataReceived += (_, e) => { action(e.Data); };
        //    //process.ErrorDataReceived += (_, e) => { throw new Exception(e.Data); };

        //    process.Start();

        //    process.BeginErrorReadLine();
        //    process.BeginOutputReadLine();

        //    process.WaitForExit();

        //}

        public static void RunGitCmd(string args)
        {
            var GitPath = ConfigurationManager.AppSettings["GitPath"];

            Trace.WriteLine(string.Format("{2}>{0} {1}", GitPath, args, ""), TRACE_CATEGORY);

            var pinfo = new ProcessStartInfo("cmd.exe")
            {
                Arguments = "/C " + Path.GetFileName(GitPath) + " " + args,
                CreateNoWindow = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(GitPath),
            };

            using (var process = Process.Start(pinfo))
            {
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error)) throw new Exception(error);
            }
        }
    }
}
