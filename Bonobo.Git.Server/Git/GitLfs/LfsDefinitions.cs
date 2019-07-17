using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git.GitLfs
{

    /// <summary> This is a list of the supported Transfer Providers.</summary>
    /// <remarks>Only the "Basic" transfer method is required t o be supported and there are no others with any notable industry support.</remarks>
    public enum LfsTransferProvider
    {
        Basic
    }

    /// <summary>
    /// Maps LfsTransferProvider values to the string representations which are to be found in LFS-HTTP requests and responses.
    /// </summary>
    public class LfsTransferProviderNames
    {
        public const string BASIC = "basic";
        public static string GetName(LfsTransferProvider transferProvider)
        {
            switch (transferProvider)
            {
                case LfsTransferProvider.Basic:
                    return BASIC;
                default:
                    throw new NotSupportedException($"LfsTransferProvider enum member not handled: {Enum.GetName(typeof(LfsTransferProvider), transferProvider)}");
            }
        }
    }

    /// <summary>
    /// The LFS operations which may be requested by an LFS client. 
    /// </summary>
    public enum LfsOperation
    {
        Download,
        Upload
    };

    /// <summary>
    /// Maps LfsOperation values to the string representations which are to be found in LFS-HTTP requests and responses.
    /// </summary>
    public class LfsOperationNames
    {
        public const string DOWNLOAD = "download";
        public const string UPLOAD = "upload";

        public static string GetName(LfsOperation operation)
        {
            switch (operation)
            {
                case LfsOperation.Download:
                    return LfsOperationNames.DOWNLOAD;
                case LfsOperation.Upload:
                    return LfsOperationNames.UPLOAD;
                default:
                    throw new NotSupportedException($"LfsOperation enum number not handled: {Enum.GetName(typeof(LfsOperation), operation)}");
            }
        }

        public static bool IsValid(string operationName)
        {
            LfsOperation[] values = (LfsOperation[]) Enum.GetValues(typeof(LfsOperation));
            return values.Any(o => LfsOperationNames.GetName(o).Equals(operationName));
        }
    }

    public static class GitLfsConsts
    {
        public const string GIT_LFS_CONTENT_TYPE = "application/vnd.git-lfs+json";
    }
}