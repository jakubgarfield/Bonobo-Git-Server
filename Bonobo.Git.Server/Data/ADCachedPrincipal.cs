using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data
{
    public class ADCachedPrincipal : IDisposable
    {
        public DateTime CacheTime { get; private set; }
        public Principal Principal { get; private set; }
        public PrincipalContext PrincipalContext { get; private set; }


        private bool Disposing;


        public ADCachedPrincipal(PrincipalContext pc, Principal principal)
        {
            CacheTime = DateTime.UtcNow;
            Disposing = false;
            Principal = principal;
            PrincipalContext = pc;
        }


        public void Dispose()
        {
            if (Disposing)
            {
                throw new ObjectDisposedException("Principal");
            }

            Disposing = true;
            Principal.Dispose();
        }
    }
}