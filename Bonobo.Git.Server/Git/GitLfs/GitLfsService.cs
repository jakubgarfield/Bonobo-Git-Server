using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Git.Models;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Bonobo.Git.Server.Git.GitLfs
{
    public class GitLfsService: IGitLfsService
    {
        [Dependency]
        public IRepositoryRepository RepositoryRepository { get; set; }

        [Dependency]
        public IRepositoryPermissionService RepositoryPermissionService { get; set; }

        [Dependency]
        public ILfsDataStorageProvider StorageProvider { get; set; }

        public Newtonsoft.Json.JsonSerializerSettings GetJsonSerializerSettings()
        {
            var contractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
            {
                NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
            };
            var jsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            };
            return jsonSerializerSettings;
        }

        public GitLfsResult GetBatchApiResponse(string urlScheme, string urlAuthority, string requestApplicationPath, string repositoryName, string[] acceptTypes, BatchApiRequest requestObj, Guid userId)
        {
            // Validate the request.
            if (!acceptTypes
                    .Select(at => at.Split(new[] { ';' }))
                    .SelectMany(list => list)
                .Any(at => at.Equals("application/vnd.git-lfs+json")))
            {
                return GitLfsResult.From(
                    Newtonsoft.Json.JsonConvert.SerializeObject(
                        new BatchApiErrorResponse() { Message = "Invalid ContentType." }),
                    406,
                    GitLfsConsts.GIT_LFS_CONTENT_TYPE);
            }

            // Check permissions
            RepositoryAccessLevel accessLevelRequested;
            if (requestObj.Operation.Equals(LfsOperationNames.DOWNLOAD)) accessLevelRequested = RepositoryAccessLevel.Pull;
            else if (requestObj.Operation.Equals(LfsOperationNames.UPLOAD)) accessLevelRequested = RepositoryAccessLevel.Push;
            else accessLevelRequested = RepositoryAccessLevel.Administer;

            bool authorized = RepositoryPermissionService.HasPermission(userId, RepositoryRepository.GetRepository(repositoryName).Id, accessLevelRequested);
            if (!authorized)
            {
                return GitLfsResult.From(
                    Newtonsoft.Json.JsonConvert.SerializeObject(
                        new BatchApiErrorResponse() { Message = "You do not have the required permissions." }),
                    403,
                    GitLfsConsts.GIT_LFS_CONTENT_TYPE);
            }

            if (requestObj == null)
            {
                return GitLfsResult.From(
                    Newtonsoft.Json.JsonConvert.SerializeObject(
                        new BatchApiErrorResponse() { Message = "Cannot parse request body." }),
                    400,
                    GitLfsConsts.GIT_LFS_CONTENT_TYPE);
            }


            // Process the request.
            var requestedTransferAdapters = requestObj.Transfers ?? (new string[] { LfsTransferProviderNames.BASIC });
            string firstSupportedTransferAdapter = requestedTransferAdapters.FirstOrDefault(t => t.Equals(LfsTransferProviderNames.BASIC));
            if (firstSupportedTransferAdapter != null)
            {
                string transferAdapterToUse = firstSupportedTransferAdapter;
                var requiredSpace = DetermineRequiredDiskSpace(requestObj);
                if (StorageProvider.SufficientSpace(requiredSpace))
                {
                    BatchApiResponse responseObj = new BatchApiResponse()
                    {
                        Transfer = transferAdapterToUse,
                        Objects = requestObj.Objects
                            .Select(ro => new { ro, ActionFactory = new LfsActionFactory() })
                            .Select(x => new BatchApiResponse.BatchApiObject()
                            {
                                Oid = x.ro.Oid,
                                Size = x.ro.Size,
                                Authenticated = true,
                                Actions = x.ActionFactory.CreateBatchApiObjectActions(urlScheme, urlAuthority, requestApplicationPath, requestObj.Operation, x.ro, repositoryName, StorageProvider)
                            })
                            .ToArray()
                    };


                    return GitLfsResult.From(responseObj, 200, GitLfsConsts.GIT_LFS_CONTENT_TYPE);
                }
                else
                {
                    return GitLfsResult.From(new BatchApiErrorResponse() { Message = "Insufficient storage space." }, 507, GitLfsConsts.GIT_LFS_CONTENT_TYPE);
                }
            }
            else
            {
                // None of the requested transfer adapters are supported.
                return GitLfsResult.From(new BatchApiErrorResponse() { Message = $"None of the requested transfer adapters are supported." }, 400, GitLfsConsts.GIT_LFS_CONTENT_TYPE);
            }
        }

        public long DetermineRequiredDiskSpace(BatchApiRequest request)
        {
            if (request.Operation.Equals(LfsOperationNames.UPLOAD))
                return request.Objects.Sum(o => o.Size);

            return 0L;
        }
    }
}