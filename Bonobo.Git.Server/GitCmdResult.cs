using System;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Web.Mvc;
using Bonobo.Git.Server.Configuration;

namespace Bonobo.Git.Server
{
    public class GitCmdResult : ActionResult
    {
        private readonly string _serviceName;
        private readonly bool _advertiseRefs;
        private readonly string _workingDir;
        private readonly string _gitPath;
        private readonly string _contentType;

        public string AdvertiseRefsContent { get; set; }

        public GitCmdResult(string contentType, string serviceName, bool advertiseRefs, string workingDir, string gitPath)
        {
            _contentType = contentType;
            _serviceName = serviceName;
            _advertiseRefs = advertiseRefs;
            _workingDir = workingDir;
            _gitPath = gitPath;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var response = context.HttpContext.Response;

            if (AdvertiseRefsContent != null && _advertiseRefs)
            {
                response.Write(AdvertiseRefsContent);
            }

            // SetNoCache
            response.AddHeader("Expires", "Fri, 01 Jan 1980 00:00:00 GMT");
            response.AddHeader("Pragma", "no-cache");
            response.AddHeader("Cache-Control", "no-cache, max-age=0, must-revalidate");

            response.BufferOutput = false;
            response.Charset = "";
            response.ContentType = _contentType;

            RunGitCmd(_serviceName, _advertiseRefs, _workingDir, _gitPath, GetInputStream(context.HttpContext.Request), response.OutputStream);
        }

        private static Stream GetInputStream(HttpRequestBase request)
        {
            return request.Headers["Content-Encoding"] == "gzip" ? new GZipStream(request.InputStream, CompressionMode.Decompress) : request.InputStream;
        }

        private static void RunGitCmd(string serviceName, bool advertiseRefs, string workingDir, string gitPath, Stream inStream, Stream outStream)
        {
            var args = serviceName + " --stateless-rpc";
            if (advertiseRefs)
            {
                args += " --advertise-refs";
            }
            args += " \"" + workingDir + "\"";

            var info = new System.Diagnostics.ProcessStartInfo(gitPath, args)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(UserConfiguration.Current.Repositories),
            };

            using (var process = System.Diagnostics.Process.Start(info))
            {
                inStream.CopyTo(process.StandardInput.BaseStream);
                process.StandardInput.Write('\0');

                var buffer = new byte[16 * 1024];
                int read;
                while ((read = process.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, read);
                    outStream.Flush();
                }

                process.WaitForExit();
            }
        }
    }
}