using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Bonobo.Git.Server.Git
{
    public class GitCmdResult : IActionResult
    {
        private readonly string contentType;
        private readonly string advertiseRefsContent;
        private readonly Action<Stream> executeGitCommand;

        public GitCmdResult(string contentType, Action<Stream> executeGitCommand)
            : this(contentType, executeGitCommand, null)
        {
        }

        public GitCmdResult(string contentType, Action<Stream> executeGitCommand, string advertiseRefsContent)
        {
            this.contentType = contentType;
            this.advertiseRefsContent = advertiseRefsContent;
            this.executeGitCommand = executeGitCommand;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var response = context.HttpContext.Response;

            // SetNoCache
            response.Headers.Add("Expires", "Fri, 01 Jan 1980 00:00:00 GMT");
            response.Headers.Add("Pragma", "no-cache");
            response.Headers.Add("Cache-Control", "no-cache, max-age=0, must-revalidate");

            //response.BufferOutput = false;
            //response.Charset = "";
            response.ContentType = contentType;

            if (advertiseRefsContent != null)
            {
                var bytes = System.Text.Encoding.ASCII.GetBytes(advertiseRefsContent);
                response.Body.Write(bytes, 0, bytes.Length);
            }



            executeGitCommand(response.Body);

            return Task.CompletedTask;
        }
    }
}