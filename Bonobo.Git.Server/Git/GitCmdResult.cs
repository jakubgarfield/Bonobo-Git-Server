using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Bonobo.Git.Server.Git
{
    public class GitCmdResult : ActionResult
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

        public override void ExecuteResult(ActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var response = context.HttpContext.Response;

            if (advertiseRefsContent != null)
            {
                response.WriteAsync(advertiseRefsContent).RunSynchronously();
            }

            // SetNoCache
            response.Headers.Add("Expires", "Fri, 01 Jan 1980 00:00:00 GMT");
            response.Headers.Add("Pragma", "no-cache");
            response.Headers.Add("Cache-Control", "no-cache, max-age=0, must-revalidate");

            //response.BufferOutput = false;
            //response.Charset = "";
            response.ContentType = contentType;

            executeGitCommand(response.Body);
        }
    }
}