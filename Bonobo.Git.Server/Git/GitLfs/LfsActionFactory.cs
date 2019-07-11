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
                var uploadAction = new BatchApiResponse.BatchApiObjectTransferAction()
                {
                    Href = storageProvider.GetFileUrl(urlScheme, urlAuthority, requestApplicationPath, 
                        operationName, repositoryName, requestObject.Oid, requestObject.Size),
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
                var downloadAction = new BatchApiResponse.BatchApiObjectTransferAction()
                {
                    Href = storageProvider.GetFileUrl(urlScheme, urlAuthority, requestApplicationPath,
                        operationName, repositoryName, requestObject.Oid, requestObject.Size),
                    Header = null,
                    Expires_in = 0x07FFFFFFF
                };
                result.Download = downloadAction;
            }

            return result;
        }
    }
}