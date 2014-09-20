using System;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Web.Mvc;
using Bonobo.Git.Server.Configuration;

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

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var response = context.HttpContext.Response;

            if (advertiseRefsContent != null)
            {
                response.Write(advertiseRefsContent);
            }

            // SetNoCache
            response.AddHeader("Expires", "Fri, 01 Jan 1980 00:00:00 GMT");
            response.AddHeader("Pragma", "no-cache");
            response.AddHeader("Cache-Control", "no-cache, max-age=0, must-revalidate");

            response.BufferOutput = false;
            response.Charset = "";
            response.ContentType = contentType;

            executeGitCommand(response.OutputStream);
        }
    }
}