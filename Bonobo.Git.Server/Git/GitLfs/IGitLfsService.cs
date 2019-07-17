using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bonobo.Git.Server.Git.Models;
using Newtonsoft.Json;

namespace Bonobo.Git.Server.Git.GitLfs
{
    /// <summary>Represents all the parts needed to construct an HTTP response.</summary>
    /// <remarks>This detaches the LFS service from HTTP for testability.</remarks>
    public class GitLfsResult
    {
        public object Content { get; set; }
        public int HttpStatusCode { get; set; }
        public string ContentType { get; set; }

        public static GitLfsResult From(object content, int httpStatusCode, string contentType)
        { 
            return new GitLfsResult()
            {
                Content = content,
                HttpStatusCode = httpStatusCode,
                ContentType = contentType
            };
        }
    }

    /// <summary>Represents the BatchAPI aspect of the LFS API standard.</summary>
    /// <remarks>Future support for Locks would also probably go here.</remarks>
    public interface IGitLfsService
    {
        Newtonsoft.Json.JsonSerializerSettings GetJsonSerializerSettings();
        GitLfsResult GetBatchApiResponse(string urlScheme, string urlAuthority, string requestApplicationPath, 
            string repositoryName, string[] acceptTypes, BatchApiRequest requestObj, Guid userId);
        long DetermineRequiredDiskSpace(BatchApiRequest request);
    }
}