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


        private bool _Disposing;


        public ADCachedPrincipal(PrincipalContext pc, Principal principal)
        {
            _Disposing = false;
            CacheTime = DateTime.UtcNow;
            Principal = principal;
            PrincipalContext = pc;
        }


        public void Dispose()
        {
            if (_Disposing)
            {
                throw new ObjectDisposedException(nameof(Principal));
            }

            _Disposing = true;
            Principal.Dispose();
        }
    }
}