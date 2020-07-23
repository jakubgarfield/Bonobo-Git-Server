using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using Serilog;
using System.Security.Principal;

namespace Bonobo.Git.Server.Helpers
{
    public static class ADHelper
    {
        private static Dictionary<Guid, ADCachedSearchResult> s_cachedMemberResults = new Dictionary<Guid, ADCachedSearchResult>();
        private static object s_cachedMemberResultsLock = new object();

        private static Dictionary<string, PrincipalContext> s_cachedPrincipalContexts = new Dictionary<string, PrincipalContext>();
        private static object s_cachedPrincipalContextsLock = new object();

        private static List<ADCachedPrincipal> s_cachedPrincipals = new List<ADCachedPrincipal>();
        private static object s_cachedPrincipalsLock = new object();

        private static readonly TimeSpan s_defaultGroupQueryCacheExpiry = new TimeSpan(0, 15, 0);
        private static readonly TimeSpan s_defaultPrincipalCacheExpiry = new TimeSpan(0, 10, 0);


        /// <summary>
        /// There are various sources of domains which we need to check
        /// Try to lazy-enumerate this, so that expensive functions aren't called if they're not necessary
        /// </summary>
        /// <param name="username">The full user name (not stripped, should contain domain if available)</param>
        /// <returns>An Enumerable of domains which can be tried</returns>
        private static IEnumerable<Domain> GetAllDomainPossibilities(string username = "")
        {
            //Skip checking username if none is supplied.
            if(!string.IsNullOrEmpty(username))
            { 
                // First we try for the domain in the username
                var parsedDomainName = username.GetDomain();
                if (!string.IsNullOrEmpty(parsedDomainName))
                {
                    var domainFromUsername = GetDomain(parsedDomainName);
                    if (domainFromUsername != null)
                    {
                        yield return domainFromUsername;
                    }
                }
                else
                {
                    Log.Verbose("AD: Username {UserName} contains no domain part", username);
                }
            }

            // The we try the domain in web.config, if there is one
            string defaultDomainName = ConfigurationManager.AppSettings["ActiveDirectoryDefaultDomain"];
            if (!string.IsNullOrEmpty(defaultDomainName))
            {
                Log.Verbose("AD: Default domain set as {DomainName}", defaultDomainName);

                var domainFromConfig = GetDomain(defaultDomainName);
                if (domainFromConfig != null)
                {
                    yield return domainFromConfig;
                    yield break;
                }
            }
            else
            {
                Log.Verbose("AD: No default domain setting in web.config");
            }

            // Finally try the global catalogue if haven't found the default domain
            GlobalCatalogCollection gcc = Forest.GetCurrentForest().FindAllGlobalCatalogs();
            Log.Information("Searching in {count} global catalogs", gcc.Count);

            // Else try all global catalogs in the current forest.
            foreach (GlobalCatalog gc in gcc)
            {
                Domain domain = null;
                try
                {
                    Log.Information("Checking GlobalCatalogue {globalCatalog}", gc.Name);
                    domain = gc.Domain;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to get domain from catalog {globalCatalog}", gc.Name);
                }
                if (domain != null)
                {
                    yield return domain;
                }
            }
        }

        /// <summary>
        /// Validates the user against Active directory - will try the domain that are part of the username if present
        /// Alternatively it tries all domains in the current forest.
        /// </summary>
        /// <param name="username">Username with or without domain</param>
        /// <param name="password"></param>
        /// <returns>True on successfull validation</returns>
        public static bool ValidateUser(string username, string password)
        {
            Log.Information("AD: Validating user {UserName}", username);

            string strippedUsername = username.StripDomain();

            foreach (var domain in GetAllDomainPossibilities(username))
            {
                Log.Information("AD: Validating stripped username {StrippedUserName} - against domain {DomainName}",
                    strippedUsername, domain.Name);

                if (ValidateUser(domain, strippedUsername, password))
                {
                    return true;
                }
            }

            Log.Information("AD: Failed to validate user {UserName}", username);

            return false;
        }

        private static bool ValidateUser(Domain domain, string username, string password)
        {
            try
            {
                Log.Information("AD: Validating {UserName} in domain {DomainName}", username, domain.Name);

                PrincipalContext pc = GetPrincipalContext(ContextType.Domain, domain.Name);

                if (pc.ValidateCredentials(username, password, ContextOptions.Negotiate))
                {
                    Log.Information("AD: Success validating {UserName} in domain {DomainName}", username, domain.Name);
                    return true;
                }
                Log.Warning("AD: Validating {UserName} in domain {DomainName} failed", username, domain.Name);
            }
            catch (Exception exp)
            {
                Log.Error(exp, "AD Validate user {username}", username);
            }
            return false;
        }

        private static Domain GetDomain(string parsedDomainName)
        {
            Log.Verbose("ADHelp: Creating directory context with domain: {DomainName}", parsedDomainName);

            Domain domain = null;

            try
            { 

                var dc = new DirectoryContext(DirectoryContextType.Domain, parsedDomainName);
                
                domain = Domain.GetDomain(dc);
            }
            catch (Exception exp)
            {
                Log.Error(exp, "Failed to create Directory context for domain {domain}.", parsedDomainName);
            }

            return domain;
        }
        /// <summary>
        /// Used to get the UserPrincpal based on username - will try the domain that are part of the username if present
        /// </summary>
        /// <param name="username">UPN or SamAccountName</param>
        /// <returns>Userprincipal if found else null</returns>
        public static UserPrincipal GetUserPrincipal(string username)
        {
            var parsedDomainName = username.GetDomain();
            string strippedUsername = username.StripDomain();

            Log.Verbose("GetUserPrincipal: username {UserName}, domain {DomainName}, stripped {StrippedUserName}", username, parsedDomainName, strippedUsername);

            lock (s_cachedPrincipalsLock)
            {
                ADCachedPrincipal cachedUserData;

                if (TryGetCachedPrincipalData(username, out cachedUserData))
                {
                    return cachedUserData.Principal as UserPrincipal;
                }

                foreach (var domain in GetAllDomainPossibilities(username))
                {
                    if (TryCacheUserPrincipalData(domain, username, strippedUsername, out cachedUserData))
                    {
                        return cachedUserData.Principal as UserPrincipal;
                    }

                    Log.Warning("Null principal in domain: {DomainName}, user: {UserName}", domain.Name,
                        strippedUsername);
                }
            }

            return null;
        }

        private static UserPrincipal GetUserPrincipal(Domain domain, string fullUsername, string strippedUsername)
        {
            var attempts = new string[] { fullUsername, strippedUsername };

            lock (s_cachedPrincipalsLock)
            {
                ADCachedPrincipal cachedUserData;

                foreach (string name in attempts)
                {
                    if (TryGetCachedPrincipalData(name, out cachedUserData))
                    {
                        return cachedUserData.Principal as UserPrincipal;
                    }
                }

                if (TryCacheUserPrincipalData(domain, fullUsername, strippedUsername, out cachedUserData))
                {
                    return cachedUserData.Principal as UserPrincipal;
                }
            }

            return null;
        }
        /// <summary>
        /// Used to get the UserPrinpal from a GUID
        /// </summary>
        /// <param name="id">The GUID of user</param>
        /// <returns>The Userprincipal if found, else null</returns>
        public static UserPrincipal GetUserPrincipal(Guid id)
        {
            lock (s_cachedPrincipalsLock)
            {
                ADCachedPrincipal cachedUserData;

                if (TryGetCachedPrincipalData(id, out cachedUserData))
                {
                    return cachedUserData.Principal as UserPrincipal;
                }

                foreach (Domain domain in GetAllDomainPossibilities())
                {
                    if (TryCacheUserPrincipalData(domain, id, out cachedUserData))
                    {
                        return cachedUserData.Principal as UserPrincipal;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Used to get the members group defined in web.config
        /// Returns the principal context to be able to do further processing on the group, ie fetch users etc.
        /// If the Principal context is disposed, it cannot query any more.
        /// </summary>
        /// <param name="group">The AD membergroup</param>
        /// <returns>Principal context on which the membersgroup was found</returns>
        public static PrincipalContext GetMembersGroup(out GroupPrincipal group)
        {
            return GetPrincipalGroup(ActiveDirectorySettings.MemberGroupName, out group);
        }
        /// <summary>
        /// Gets a principal group by name
        /// Returns the principal context to be able to do further processing on the group, ie fetch users etc.
        /// If the Principal context is disposed, it cannot query any more.
        /// </summary>
        /// <param name="name">The group to search for</param>
        /// <param name="group">The group found</param>
        /// <returns>Principal context on which the group was found.</returns>
        public static PrincipalContext GetPrincipalGroup(string name, out GroupPrincipal group)
        {
            lock (s_cachedPrincipalsLock)
            {
                ADCachedPrincipal cachedGroupData;

                if (TryGetCachedPrincipalData(name, out cachedGroupData))
                {
                    group = cachedGroupData.Principal as GroupPrincipal;
                    return cachedGroupData.PrincipalContext;
                }

                foreach (Domain domain in GetAllDomainPossibilities())
                {
                    if (TryCacheGroupPrincipalData(domain, name, out cachedGroupData))
                    {
                        group = cachedGroupData.Principal as GroupPrincipal;
                        return cachedGroupData.PrincipalContext;
                    }
                }
            }

            throw new ArgumentException("Could not find principal group: " + name);
        }


        public static IList<Principal> GetGroupMembers(GroupPrincipal group)
        {
            var      groupGuid  = (Guid) group.Guid;
            TimeSpan cacheExpiry =
                ConfigurationHelper.ParseTimeSpanOrDefault(
                    ConfigurationManager.AppSettings["ActiveDirectoryGroupQueryCacheExpiry"],
                    s_defaultGroupQueryCacheExpiry
                );

            lock (s_cachedMemberResultsLock)
            {
                Log.Verbose("GetGroupMembers of {Group}", group.SamAccountName);

                if (s_cachedMemberResults.ContainsKey(groupGuid))
                {
                    var results = s_cachedMemberResults[groupGuid];

                    if (DateTime.UtcNow.Subtract(cacheExpiry) > results.CacheTime)
                    {
                        Log.Verbose("ADCACHE: Cache expired for results of {Guid}", groupGuid);

                        results.Dispose();
                        s_cachedMemberResults.Remove(groupGuid);
                    }
                    else
                    {
                        Log.Verbose("ADCACHE: Cache hit for results of {Guid}", groupGuid);
                        return results.Principals;
                    }
                }

                Log.Verbose("ADCACHE: Cache miss for results of {Guid}", groupGuid);

                var newCache = new ADCachedSearchResult(group.GetMembers(true));
                s_cachedMemberResults.Add(groupGuid, newCache);

                return newCache.Principals;
            }
        }

        private static PrincipalContext GetPrincipalContext(ContextType contextType, string name)
        {
            string safeName = name.ToLower();

            lock (s_cachedPrincipalContextsLock)
            {
                if (s_cachedPrincipalContexts.ContainsKey(safeName))
                {
                    Log.Verbose("ADCACHE: Cache hit for context {Domain}", name);
                    return s_cachedPrincipalContexts[safeName];
                }

                Log.Verbose("ADCACHE: Cache miss for context {Domain}", name);

                var pc = new PrincipalContext(contextType, name);

                s_cachedPrincipalContexts.Add(safeName, pc);

                return pc;
            }
        }

        private static bool TryCacheGroupPrincipalData(Domain domain, string name, out ADCachedPrincipal cacheData)
        {
            cacheData = null;

            try
            {
                var pc = new PrincipalContext(ContextType.Domain, domain.Name);

                Log.Information("Searching for group {name} in domain {domain}", name, domain.Name);

                var group = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, name);

                if (group != null)
                {
                    cacheData = new ADCachedPrincipal(pc, group);
                    s_cachedPrincipals.Add(cacheData);
                    return true;
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, "GetPrincipal Group with name: " + name);
                // let it fail
            }

            return false;
        }

        private static bool TryCacheUserPrincipalData(Domain domain, Guid id, out ADCachedPrincipal cacheData)
        {
            cacheData = null;

            try
            {
                PrincipalContext pc = GetPrincipalContext(ContextType.Domain, domain.Name);

                Log.Information("Looking for user with guid {guid} in domain {domain}", id.ToString(), domain.Name);

                var user = UserPrincipal.FindByIdentity(pc, IdentityType.Guid, id.ToString());
                if (user != null)
                {
                    cacheData = new ADCachedPrincipal(pc, user);
                    s_cachedPrincipals.Add(cacheData);
                    return true;
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, "AD: Failed to find user with guid {GUID}", id.ToString());
                // let it fail
            }

            return false;
        }

        private static bool TryCacheUserPrincipalData(Domain domain, string fullUsername, string strippedUsername, out ADCachedPrincipal cacheData)
        {
            cacheData = null;
            
            try
            {
                PrincipalContext pc = GetPrincipalContext(ContextType.Domain, domain.Name);

                UserPrincipal principalBySamName = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, strippedUsername);
                if (principalBySamName != null)
                {
                    cacheData = new ADCachedPrincipal(pc, principalBySamName);
                    s_cachedPrincipals.Add(cacheData);
                    return true;
                }

                Log.Verbose("TryCachePrincipalData: Did not find user {UserName} in domain {DomainName} by SamAccountName", strippedUsername, domain.Name);

                UserPrincipal principalByUPN = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, fullUsername);
                if (principalByUPN != null)
                {
                    cacheData = new ADCachedPrincipal(pc, principalByUPN);
                    s_cachedPrincipals.Add(cacheData);
                    return true;
                }

                Log.Verbose(
                    "TryCachePrincipalData: Did not find user {UserName} in domain {DomainName} by UPN",
                    fullUsername, domain.Name);
            }
            catch (Exception exp)
            {
                Log.Error(exp, "TryCachePrincipalData in domain: {DomainName}, user: {FullUserName} ({StrippedUserName})", domain.Name, fullUsername, strippedUsername);
            }

            return false;
        }

        private static bool TryGetCachedPrincipalData(Guid guid, out ADCachedPrincipal cacheData)
        {
            TimeSpan cacheExpiry =
                ConfigurationHelper.ParseTimeSpanOrDefault(
                    ConfigurationManager.AppSettings["ActiveDirectoryPrincipalCacheExpiry"],
                    s_defaultPrincipalCacheExpiry
                );

            for (int i = s_cachedPrincipals.Count - 1; i >= 0; i--)
            {
                ADCachedPrincipal dataSet = s_cachedPrincipals[i];

                if (DateTime.UtcNow.Subtract(cacheExpiry) > dataSet.CacheTime)
                {
                    Log.Verbose("ADCACHE: Cache expired for {Guid}", guid);

                    dataSet.Dispose();
                    s_cachedPrincipals.RemoveAt(i);
                    continue;
                }

                if (dataSet.Principal.Guid == guid)
                {
                    Log.Verbose("ADCACHE: Cache hit for {Guid}", guid);

                    cacheData = dataSet;
                    return true;
                }
            }

            Log.Verbose("ADCACHE: Cache miss for {Guid}", guid);

            cacheData = null;
            return false;
        }

        private static bool TryGetCachedPrincipalData(string name, out ADCachedPrincipal cacheData)
        {
            bool searchUpn = name.Contains("@");
            TimeSpan cacheExpiry =
                ConfigurationHelper.ParseTimeSpanOrDefault(
                    ConfigurationManager.AppSettings["ActiveDirectoryPrincipalCacheExpiry"],
                    s_defaultPrincipalCacheExpiry
                );

            for (int i = s_cachedPrincipals.Count - 1; i >= 0; i--)
            {
                ADCachedPrincipal dataSet = s_cachedPrincipals[i];

                if (DateTime.UtcNow.Subtract(cacheExpiry) > dataSet.CacheTime)
                {
                    Log.Verbose("ADCACHE: Cache expired for {Name}", name);

                    dataSet.Dispose();
                    s_cachedPrincipals.RemoveAt(i);
                    continue;
                }

                if (searchUpn && !string.IsNullOrEmpty(dataSet.Principal.UserPrincipalName))
                {
                    if (dataSet.Principal.UserPrincipalName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Log.Verbose("ADCACHE: Cache hit for {UPN}", name);

                        cacheData = dataSet;
                        return true;
                    }
                }
                else
                {
                    if (dataSet.Principal.SamAccountName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Log.Verbose("ADCACHE: Cache hit for {SamAccountName}", name);

                        cacheData = dataSet;
                        return true;
                    }
                }
            }

            Log.Verbose("ADCACHE: Cache miss for {Name}", name);

            cacheData = null;
            return false;
        }
    }
}