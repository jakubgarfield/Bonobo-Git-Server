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
        private static Dictionary<string, PrincipalContext> CachedPrincipalContexts = new Dictionary<string, PrincipalContext>();
        private static object CachedPrincipalContextsLock = new object();

        private static List<ADCachedPrincipal> CachedPrincipals = new List<ADCachedPrincipal>();
        private static object CachedPrincipalsLock = new object();


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

            var cachedUserData = GetCachedPrincipalData(username);

            if (cachedUserData != null)
                return cachedUserData.Principal as UserPrincipal;

            foreach (var domain in GetAllDomainPossibilities(username))
            {
                var user = GetUserPrincipal(domain, username, strippedUsername);
                if (user != null)
                    return user;
                Log.Warning("Null principal in domain: {DomainName}, user: {UserName}", domain.Name,
                    strippedUsername);
            }

            return null;
        }

        private static UserPrincipal GetUserPrincipal(Domain domain, string fullUsername, string strippedUsername)
        {
            var attempts = new string[] { fullUsername, strippedUsername };

            foreach (string name in attempts)
            {
                var cachedUser = GetCachedPrincipalData(name);

                if (cachedUser != null)
                    return cachedUser.Principal as UserPrincipal;
            }

            try
            {
                PrincipalContext pc = GetPrincipalContext(ContextType.Domain, domain.Name);

                UserPrincipal principalBySamName = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, strippedUsername);
                if (principalBySamName != null)
                {
                    CachedPrincipals.Add(new ADCachedPrincipal(pc, principalBySamName));
                    return principalBySamName;
                }

                Log.Verbose("GetUserPrincipal: Did not find user {UserName} in domain {DomainName} by SamAccountName", strippedUsername, domain.Name);

                UserPrincipal principalByUPN = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, fullUsername);
                if (principalByUPN == null)
                {
                    Log.Verbose(
                        "GetUserPrincipal: Did not find user {UserName} in domain {DomainName} by UPN",
                        fullUsername, domain.Name);
                }
                CachedPrincipals.Add(new ADCachedPrincipal(pc, principalByUPN));
                return principalByUPN;
            }
            catch (Exception exp)
            {
                Log.Error(exp, "GetUserPrincipal in domain: {DomainName}, user: {FullUserName} ({StrippedUserName})", domain.Name, fullUsername, strippedUsername);
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
            var cachedUser = GetCachedPrincipalData(id);

            if (cachedUser != null)
                return cachedUser.Principal as UserPrincipal;

            foreach (Domain domain in GetAllDomainPossibilities())
            {
                try
                {
                    PrincipalContext pc = GetPrincipalContext(ContextType.Domain, domain.Name);

                    Log.Information("Looking for user with guid {guid} in domain {domain}", id.ToString(), domain.Name);

                    var user = UserPrincipal.FindByIdentity(pc, IdentityType.Guid, id.ToString());
                    if (user != null)
                    {
                        CachedPrincipals.Add(new ADCachedPrincipal(pc, user));
                        return user;
                    }
                }
                catch (Exception exp)
                {
                    Log.Error(exp, "AD: Failed to find user with guid {GUID}", id.ToString());
                    // let it fail
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
            var cachedGroupData = GetCachedPrincipalData(name);

            if (cachedGroupData != null)
            {
                group = cachedGroupData.Principal as GroupPrincipal;
                return cachedGroupData.PrincipalContext;
            }

            foreach (Domain domain in GetAllDomainPossibilities())
            {
                Log.Information("Searching for group {name} in domain {domain}", name, domain.Name);

                try
                {
                    var pc = new PrincipalContext(ContextType.Domain, domain.Name);
                    group = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, name);
                    if (group != null)
                    {
                        CachedPrincipals.Add(new ADCachedPrincipal(pc, group));
                        return pc;
                    }
                }
                catch (Exception exp)
                {
                    Log.Error(exp, "GetPrincipal Group with name: " + name);
                    // let it fail
                }
            }
            throw new ArgumentException("Could not find principal group: " + name);
        }


        private static PrincipalContext GetPrincipalContext(ContextType contextType, string name)
        {
            string safeName = name.ToLower();

            lock (CachedPrincipalContextsLock)
            {
                if (CachedPrincipalContexts.ContainsKey(safeName))
                {
                    Log.Verbose("ADCACHE: Cache hit for context {Domain}", name);
                    return CachedPrincipalContexts[safeName];
                }

                Log.Verbose("ADCACHE: Cache miss for context {Domain}", name);

                var pc = new PrincipalContext(contextType, name);

                CachedPrincipalContexts.Add(safeName, pc);

                return pc;
            }
        }

        private static ADCachedPrincipal GetCachedPrincipalData(Guid guid)
        {
            TimeSpan tenMinutes = new TimeSpan(0, 10, 0);

            lock (CachedPrincipalsLock)
            {
                for (int i = CachedPrincipals.Count - 1; i >= 0; i--)
                {
                    ADCachedPrincipal dataSet = CachedPrincipals[i];

                    if (DateTime.UtcNow.Subtract(tenMinutes) > dataSet.CacheTime)
                    {
                        Log.Verbose("ADCACHE: Cache expired for {Guid}", guid);

                        dataSet.Principal.Dispose();
                        CachedPrincipals.RemoveAt(i);
                        continue;
                    }

                    if (dataSet.Principal.Guid == guid)
                    {
                        Log.Verbose("ADCACHE: Cache hit for {Guid}", guid);
                        return dataSet;
                    }
                }
            }

            Log.Verbose("ADCACHE: Cache miss for {Guid}", guid);

            return null;
        }

        private static ADCachedPrincipal GetCachedPrincipalData(string name)
        {
            TimeSpan tenMinutes = new TimeSpan(0, 10, 0);
            bool     searchUpn  = name.Contains("@");

            lock (CachedPrincipalsLock)
            {
                for (int i = CachedPrincipals.Count - 1; i >= 0; i--)
                {
                    ADCachedPrincipal dataSet = CachedPrincipals[i];

                    if (DateTime.UtcNow.Subtract(tenMinutes) > dataSet.CacheTime)
                    {
                        Log.Verbose("ADCACHE: Cache expired for {Name}", name);

                        dataSet.Principal.Dispose();
                        CachedPrincipals.RemoveAt(i);
                        continue;
                    }

                    if (searchUpn && !string.IsNullOrEmpty(dataSet.Principal.UserPrincipalName))
                    {
                        if (dataSet.Principal.UserPrincipalName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            Log.Verbose("ADCACHE: Cache hit for {UPN}", name);
                            return dataSet;
                        }
                    }
                    else
                    {
                        if (dataSet.Principal.SamAccountName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            Log.Verbose("ADCACHE: Cache hit for {SamAccountName}", name);
                            return dataSet;
                        }
                    }
                }
            }

            Log.Verbose("ADCACHE: Cache miss for {Name}", name);

            return null;
        }
    }
}