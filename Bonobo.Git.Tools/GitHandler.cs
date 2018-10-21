using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace Bonobo.Git.Tools
{
    public class GitHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        private string GitPath, gitWorkingDir, gitBaseDir;

        private HttpContext context;

        public void ProcessRequest(HttpContext context)
        {
            this.context = context;

            if (!HasAccess()) return;

            GitPath = ConfigurationManager.AppSettings["GitPath"];
            gitBaseDir = ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"];
            gitWorkingDir = GetGitDir(context.Request.RawUrl);

            if(string.IsNullOrEmpty(gitWorkingDir) || 
               !File.Exists(GitPath) ||
               !Directory.Exists(Path.Combine(gitBaseDir, gitWorkingDir)))
            {
                context.Response.StatusCode = 400;
                context.Response.End();
                return;
            }

            gitWorkingDir = Path.Combine(gitBaseDir, gitWorkingDir);

            if (context.Request.RawUrl.IndexOf("/info/refs?service=git-receive-pack") >= 0)
            {
                GetInfoRefs("receive-pack");
            }
            else if (context.Request.RawUrl.IndexOf("/git-receive-pack") >= 0 && context.Request.RequestType == "POST")
            {
                try
                {
                    ServiceRpc("receive-pack");
                }
                finally
                {
                    Git.Run(@"update-server-info", gitWorkingDir);
                }
            }
            else if (context.Request.RawUrl.IndexOf("/info/refs?service=git-upload-pack") >= 0)
            {
                GetInfoRefs("upload-pack");
            }
            else if (context.Request.RawUrl.IndexOf("/git-upload-pack") >= 0 && context.Request.RequestType == "POST")
            {
                ServiceRpc("upload-pack");
            }
        }

        public string GetGitDir(string rawUrl)
        {
            //var match = Regex.Match(rawUrl, "/(.[^\\.]+.git)");
            //var path = match.Success ? match.Groups[1].Value : "";
            //return Path.GetFileName(path);

            var path = rawUrl.Substring(0, rawUrl.IndexOf(".git") + 4);
            return path.StartsWith("/") ? path.Substring(1) : path;
           
        }

        /// <summary>
        /// for to test with fiddler
        /// export http_proxy=http://localhost:8888
        /// </summary>
        /// <returns></returns>
        private bool HasAccess()
        {
            var authMode = "none";

            if (string.IsNullOrEmpty(authMode) || string.Compare(authMode, "none", true) == 0) return true;

            if (string.Compare(authMode, "all", true) == 0 || context.Request.RawUrl.IndexOf("git-receive-pack") >= 0)
            {
                string authHeader = context.Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authHeader))
                {
                    context.Response.StatusCode = 401;
                    context.Response.AddHeader("WWW-Authenticate", "Basic");
                    return false;
                }
                else
                {
                    try
                    {
                        string userNameAndPassword = Encoding.Default.GetString(
                            Convert.FromBase64String(authHeader.Substring(6)));
                        string[] parts = userNameAndPassword.Split(':');
                        var username = parts[0];
                        var password = parts[1];
                        var gitWorkingDir = GetGitDir(context.Request.RawUrl);
                        
                        return username == gitWorkingDir.Substring(0, gitWorkingDir.IndexOf("/")) &&
                               System.Web.Security.Membership.ValidateUser(username, password);

                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// "transfer-encoding:chunked is not supported. 
        /// Workaround: Set 'git config --add --global http.postBuffer 10485760'
        /// </summary>
        /// <param name="serviceName"></param>
        private void ServiceRpc(string serviceName)
        {
            context.Response.ContentType = string.Format("application/x-git-{0}-result", serviceName);

            var fin = Path.GetTempFileName();
            var fout = Path.GetTempFileName();

            using (var file = File.Create(fin))
            {
                var encoding = context.Request.Headers["Content-Encoding"];
                if (string.IsNullOrEmpty(encoding))
                    encoding = context.Request.ContentEncoding.EncodingName;

                if (encoding.Equals("gzip"))
                {
                    using (GZipStream decomp = new GZipStream(context.Request.InputStream, CompressionMode.Decompress))
                    {
                        decomp.CopyTo(file);
                    }
                }
                else
                {
                    context.Request.InputStream.CopyTo(file);
                }
            }

            Git.RunGitCmd(string.Format("{0} --stateless-rpc \"{1}\" < \"{2}\" > \"{3}\"", serviceName, gitWorkingDir, fin, fout));
            PutFileInChunks(fout, context.Response);
            File.Delete(fin);
            File.Delete(fout);
            context.Response.End();
        }

        private void GetInfoRefs(string serviceName)
        {
            var fout = Path.GetTempFileName();
            context.Response.ContentType = string.Format("application/x-git-{0}-advertisement", serviceName);
            context.Response.Charset = null;
            context.Response.Write(GitString("# service=git-" + serviceName + "\n"));
            Git.RunGitCmd(string.Format("{0} --stateless-rpc --advertise-refs \"{1}\" > \"{2}\"", serviceName, gitWorkingDir, fout));
            PutFileInChunks(fout, context.Response);
            context.Response.Write("0000");
            File.Delete(fout);
            context.Response.End();
        }

        private void PutFileInChunks(string filename, HttpResponse response)
        {
            const int chunkSize = 16384;
            var buffer = new byte[chunkSize];
            using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int read = 0;
                while ((read = fs.Read(buffer, 0, chunkSize)) > 0)
                {
                    response.OutputStream.Write(buffer, 0, read);
                    response.Flush();
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private string GitString(string s)
        {
            var len = (s.Length + 4).ToString("x");
            while (len.Length < 4) len = "0" + len;
            return len + s;
        }

        private void WriteNoCache(HttpContext context)
        {
            context.Response.AddHeader("Expires", "Fri, 01 Jan 1980 00:00:00 GMT");
            context.Response.AddHeader("Pragma", "no-cache");
            context.Response.AddHeader("Cache-Control", "no-cache, max-age=0, must-revalidate");
        }

        #endregion
    }
}
