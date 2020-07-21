using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data
{
    public class ADCachedPrincipal
    {
        public DateTime CacheTime { get; private set; }
        public Principal Principal { get; private set; }
        public PrincipalContext PrincipalContext { get; private set; }


        public ADCachedPrincipal(PrincipalContext pc, Principal principal)
        {
            CacheTime = DateTime.UtcNow;
            Principal = principal;
            PrincipalContext = pc;
        }
    }
}