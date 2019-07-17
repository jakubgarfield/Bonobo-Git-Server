using Bonobo.Git.Server.Git.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitLfs
{
    /// <summary> Factory for producing actions of the correct type per the specified operation.  Used when constructing LFS responses. </summary>
    public class LfsActionFactory
    {

        /// <summary>Creates the ACTION part of the response object for the given operation. </summary>
        /// <param name="urlScheme">The protocol part of the URL.</param>
        /// <param name="urlAuthority">The domain part of the URL.</param>
        /// <param name="requestApplicationPath">The path part of the URL.</param>
        /// <param name="operationName">The requested operation specified in the LFS request.</param>
        /// <param name="requestObject">The LFS request.</param>
        /// <param name="repositoryName">The repository to which this request applies.</param>
        /// <param name="storageProvider">The storage provider to use for managing LFS data.</param>
        /// <returns></returns>
        public BatchApiResponse.BatchApiObjectAction CreateBatchApiObjectActions(
            string urlScheme, string urlAuthority, string requestApplicationPath, 
            string operationName, BatchApiRequest.LfsObjectToTransfer requestObject, string repositoryName, ILfsDataStorageProvider storageProvider)
        {
            if (!LfsOperationNames.IsValid(operationName))
                throw new NotSupportedException($"Invalid operation name ({operationName})");

            var result = new BatchApiResponse.BatchApiObjectAction();
            // If this is an upload, generate an upload action.
            if (operationName.Equals(LfsOperationNames.UPLOAD))
            {
                string fileUrl = FileUrl(urlScheme, urlAuthority, ref requestApplicationPath, requestObject, repositoryName);

                var uploadAction = new BatchApiResponse.BatchApiObjectTransferAction()
                {
                    Href = fileUrl,
                    Header = null,
                    Expires_in = 0x07FFFFFFF
                };
                result.Upload = uploadAction;
            }

            // If this is an upload, we *may* generate a verify action here.
            if (operationName.Equals(LfsOperationNames.UPLOAD))
            {
                // Optional and not yet implemented.
            }

            // If this is a download, gnerate a download action.
            if (operationName.Equals(LfsOperationNames.DOWNLOAD))
            {
                string fileUrl = FileUrl(urlScheme, urlAuthority, ref requestApplicationPath, requestObject, repositoryName);

                var downloadAction = new BatchApiResponse.BatchApiObjectTransferAction()
                {
                    Href = fileUrl,
                    Header = null,
                    Expires_in = 0x07FFFFFFF
                };
                result.Download = downloadAction;
            }

            return result;
        }

        private string FileUrl(string urlScheme, string urlAuthority, ref string requestApplicationPath, BatchApiRequest.LfsObjectToTransfer requestObject, string repositoryName)
        {
            var authorityParts = urlAuthority.Split(':');
            var path = requestApplicationPath = string.Concat(
                repositoryName,
                ".git",
                requestApplicationPath,
                "lfs/oid/",
                requestObject.Oid);

            var ub = new UriBuilder();
            ub.Scheme = urlScheme;
            ub.Host = authorityParts[0];
            if (authorityParts.Length > 1)
                if (int.TryParse(authorityParts[1], out int iport))
                    ub.Port = iport;
            ub.Path = path;
            string fileUrl = ub.ToString();
            return fileUrl;
        }
    }
}