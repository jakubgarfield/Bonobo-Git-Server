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


        private bool _disposing;


        public ADCachedPrincipal(PrincipalContext pc, Principal principal)
        {
            _disposing = false;
            CacheTime = DateTime.UtcNow;
            Principal = principal;
            PrincipalContext = pc;
        }


        public void Dispose()
        {
            if (_disposing)
            {
                throw new ObjectDisposedException(nameof(ADCachedPrincipal));
            }

            _disposing = true;
            Principal.Dispose();
        }
    }
}