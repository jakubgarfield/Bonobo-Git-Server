using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data
{
    public class ADCachedSearchResult : IDisposable
    {
        public DateTime CacheTime { get; private set; }
        public IList<Principal> Principals { get; private set; }
        public PrincipalSearchResult<Principal> SearchResults { get; private set; }


        private bool Disposing;


        public ADCachedSearchResult(PrincipalSearchResult<Principal> searchResults)
        {
            CacheTime = DateTime.UtcNow;
            Disposing = false;
            SearchResults = searchResults;

            // Search results are lazy loaded, we should enum them now so the cache is
            // effective
            //
            var principals = new List<Principal>();

            foreach (Principal principal in SearchResults)
            {
                principals.Add(principal);
            }

            Principals = principals.AsReadOnly();
        }


        public void Dispose()
        {
            if (Disposing)
            {
                throw new ObjectDisposedException("SearchResults");
            }

            Disposing = true;

            foreach (Principal principal in Principals)
            {
                principal.Dispose();
            }

            SearchResults.Dispose();
        }
    }
}