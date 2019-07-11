using System;
using System.Xml;

namespace Bonobo.Git.Server.Git.Models
{

    /// <summary> This represents the response which is sent back from Batch API. </summary>
    public class BatchApiResponse
    {
        public class ActionHeader
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public class BatchApiObjectTransferAction
        {
            private DateTime? _expiresAt;
            private DateTime? FromRfc3339(string rfc3339)
            {
                return (rfc3339 == null)
                    ? (DateTime?)null
                    : XmlConvert.ToDateTime(rfc3339??"", XmlDateTimeSerializationMode.Local);

            }

            private string ToRfc3339(DateTime? dateTime)
            {
                return (dateTime == null) 
                    ? null 
                    : XmlConvert.ToString(dateTime ?? DateTime.MinValue, XmlDateTimeSerializationMode.Local);
            }

            /// <summary> The URL from which to download the object. </summary>
            public string Href { get; set; }
            /// <summary> HTTP headers which the client should add to the transfer request. </summary>
            public ActionHeader Header { get; set; }
            /// <summary> Whole number of seconds after local client time when the transfer will expire.  Preferred over Expired_At if both are provided. </summary>
            public int? Expires_in { get; set; }
            /// <summary> Uppercase RFC-3339 formatted timestamp with second precision for when the given action expires (usually due to a temporary token). </summary>
            public string Expires_at { get => ToRfc3339(_expiresAt); set => _expiresAt = FromRfc3339(value); }
        }

        public class BatchApiObjectErrorAction
        {
            public int Code { get; set; }
            public string Message { get; set; }
        }

        public class BatchApiObjectAction
        {
            private BatchApiObjectTransferAction _download;
            private BatchApiObjectTransferAction _upload;
            private BatchApiObjectTransferAction _verify;
            private BatchApiObjectErrorAction _error;

            private void ClearAll()
            {
                _download = null;
                _upload = null;
                _verify = null;
                _error = null;
            }
            private void ClearUpload()
            {
                _download = null;
                _error = null;
            }

            /// <summary> DOWNLOAD next-action object. </summary>
            /// <remarks> Mutually excluisive with Upload. </remarks>
            public BatchApiObjectTransferAction Download { get => _download; set { ClearAll(); _download = value; } }

            /// <summary> This error is per-download.  We still return an HTTP 200 in this case. </summary>
            public BatchApiObjectErrorAction Error { get => _error; set { ClearAll(); _error = value; } }

            /// <summary> UPLOAD next-action object. </summary>
            /// <remarks> Mutually excluisive with Download. </remarks>
            public BatchApiObjectTransferAction Upload { get => _upload; set { ClearUpload(); _upload = value; } }
            public BatchApiObjectTransferAction Verify { get => _verify; set { ClearUpload(); _verify = value; } }

        }

        public class BatchApiObject
        {
            /// <summary> The OID of the LFS object. </summary>
            public string Oid { get; set; }
            /// <summary> The integer byte size of the LFS obejct.  Must be at least 0. </summary>
            public long Size { get; set; }
            /// <summary> Optional.  Specifies whether the request for this specific object is authenticated. If omitted or false, Git LFS will attempt to find credntials for this URL. </summary>
            public bool Authenticated { get; set; }
            /// <summary> Contains the next actionsfor this object.  Appliable actions depend on which OPERATION is specified in the request.  These are interpreted by the transfer adapter. </summary>
            public BatchApiObjectAction Actions { get; set; }
        }

        /// <summary> Specifies the transfer adapter which the server prefers.  This MUST be one of the given TRANSFER adapters from the request. </summary>
        /// <remarks> For now, Bonobo only supports the BASIC transfer adapter. </remarks>
        public string Transfer { get; set; }
        /// <summary> An array of objects to be downloaded. </summary>
        public BatchApiObject[] Objects { get; set; }
    }

    public class BatchApiErrorResponse
    {
        public string Message { get; set; }
        public string Documentation_Url => "https://github.com/git-lfs/git-lfs/blob/master/docs/api/batch.md";
        public string Request_Id { get; set; }
    }
}
