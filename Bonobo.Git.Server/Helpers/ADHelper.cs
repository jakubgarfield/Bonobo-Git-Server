using Bonobo.Git.Server.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using Serilog;

namespace Bonobo.Git.Server.Helpers
{
    public static class ADHelper
    {
        /// <summary>
        /// There are various sources of domains which we need to check
        /// Try to lazy-enumerate this, so that expensive functions aren't called if they're not necessary
        /// </summary>
        /// <param name="username">The full user name (not stripped, should contain domain if available)</param>
        /// <returns>An Enumerable of domains which can be tried</returns>
        private static IEnumerable<Domain> GetAllDomainPossibilities(string username)
        {
            // First we try for the domain in the username
            var parsedDomainName = username.GetDomain();
            var domainFromUsername = GetDomain(parsedDomainName);
            if (domainFromUsername != null)
            {
                yield return domainFromUsername;
            }

            // The we try the domain in web.config, if there is one
            string defaultDomainName = ConfigurationManager.AppSettings["ActiveDirectoryDefaultDomain"];
            if (!string.IsNullOrEmpty(defaultDomainName))
            {
                var domainFromConfig = GetDomain(defaultDomainName);
                if (domainFromConfig != null)
                {
                    yield return domainFromConfig;
                }
            }

            // Finally try the global catalogue
            GlobalCatalogCollection gcc = Forest.GetCurrentForest().FindAllGlobalCatalogs();
            Log.Information("Searching in {count} global catalogs", gcc.Count);

            // Else try all global catalogs in the current forest.
            foreach (GlobalCatalog gc in gcc)
            {
                Log.Information("Trying GlobalCatalogue {globalCatalog} domain {domain}", gc.Name, gc.Domain.Name);
                yield return gc.Domain;
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

                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domain.Name))
                {
                    if (pc.ValidateCredentials(username, password, ContextOptions.Negotiate))
                    {
                        Log.Information("AD: Success validating {UserName} in domain {DomainName}", username, domain.Name);
                        return true;
                    }
                    Log.Warning("AD: Validating {UserName} in domain {DomainName} failed", username, domain.Name);
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, "AD Validate user {username}", username);

                Trace.TraceError(exp.Message);
                if (exp.InnerException != null)
                    Trace.TraceError(exp.InnerException.Message);
            }
            return false;
        }

        private static Domain GetDomain(string parsedDomainName)
        {
            Log.Verbose("ADHelp: Creating directory context with domain: {DomainName}", parsedDomainName);

            Domain domain = null;

            try { 

                var dc = new DirectoryContext(DirectoryContextType.Domain, parsedDomainName);

                domain = Domain.GetDomain(dc);
            }catch(Exception exp)
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

            foreach (var domain in GetAllDomainPossibilities(username))
            {
                var user = GetUserPrincipal(domain, strippedUsername);
                if (user != null)
                    return user;
                Log.Warning("Null principal in domain: {DomainName}, user: {UserName}", domain.Name,
                    strippedUsername);
            }

            return null;
        }

        private static UserPrincipal GetUserPrincipal(Domain domain, string username)
        {
            try
            {
                using (var pc = new PrincipalContext(ContextType.Domain, domain.Name))
                {
                    return UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName,username);
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, "GetUserPrincipal in domain: {DomainName}, user: {UserName}", domain.Name, username);
                Trace.TraceError("GetUserPrincipal in domain: " + domain.Name + " with username " + username);
                Trace.TraceError(exp.Message);
                if (exp.InnerException != null)
                    Trace.TraceError(exp.InnerException.Message);
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
            foreach (GlobalCatalog gc in Forest.GetCurrentForest().FindAllGlobalCatalogs())
            {
                Domain domain = gc.Domain;
                try
                {
                    using (var pc = new PrincipalContext(ContextType.Domain, domain.Name))
                    {
                        var user = UserPrincipal.FindByIdentity(pc, IdentityType.Guid, id.ToString());
                        if (user != null)
                            return user;
                    }
                }
                catch (Exception exp)
                {
                    Trace.TraceError("GetUserPrincipal GUID " + id.ToString());
                    Trace.TraceError(exp.Message);
                    if (exp.InnerException != null)
                        Trace.TraceError(exp.InnerException.Message);
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
            GlobalCatalogCollection gcc = Forest.GetCurrentForest().FindAllGlobalCatalogs();

            Log.Information("Searching in {count} global catalogs", gcc.Count);

            foreach (GlobalCatalog gc in gcc)
            {
                Log.Information("Searching for user in globalcatalog {globalCatalog} in domain {domain}", gc.Name, gc.Domain.Name);

                Domain domain = gc.Domain;

                try
                {
                    var pc = new PrincipalContext(ContextType.Domain, domain.Name);
                    group = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, name);
                    if (group != null)
                        return pc;
                }
                catch (Exception exp)
                {
                    Log.Error(exp, "GetPrincipal Group with name: " + name);
                    Trace.TraceError("GetPrincipal Group with name: " + name);
                    Trace.TraceError(exp.Message);
                    if (exp.InnerException != null)
                    {
                        Log.Error(exp.InnerException, "InnerEx on GetPrincipal Group with name: " + name);
                        Trace.TraceError(exp.InnerException.Message);
                    }
                    // let it fail
                }
            }
            throw new ArgumentException("Could not find principal group: " + name);
        }
    }
}