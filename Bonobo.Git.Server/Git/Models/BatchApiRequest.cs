using System.Web.Mvc;

namespace Bonobo.Git.Server.Git.Models
{

    [ModelBinder(typeof(BatchApiRequestModelBinder))]
    public class BatchApiRequest
    {
        public class BatchApiRef
        {
            public string Name { get; set; }
        }

        public class LfsObjectToTransfer
        {
            public string Oid { get; set; }
            public long Size { get; set; }
        }

        public string Operation { get; set; }
        public string[] Transfers { get; set; }
        public BatchApiRef Ref { get; set; }
        public LfsObjectToTransfer[] Objects { get; set; }
    }
}