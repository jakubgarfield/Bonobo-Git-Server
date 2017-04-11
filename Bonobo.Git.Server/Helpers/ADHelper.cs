using Bonobo.Git.Server.Configuration;
using System;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using Serilog;

namespace Bonobo.Git.Server.Helpers
{
    public static class ADHelper
    {
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

            var parsedDomain = username.GetDomain();
            string strippedUsername = username.StripDomain();

            Log.Information("AD: Validating user {UserName} - domain {DomainName}, stripped {StrippedUsername}", 
                username, parsedDomain, strippedUsername);

            Domain matchedDomain = GetDomain(parsedDomain);
            // If a domain was present in the supplied username, try to find this first and validate against it.
            if(matchedDomain != null)
            {
                Log.Information("AD: Found {parsedDomain}", parsedDomain);
                return ValidateUser(matchedDomain, strippedUsername, password);
            }
            // Else try all domains in the current forest.
            foreach (Domain domain in Forest.GetCurrentForest().Domains)
            {
                Log.Information("AD: Checking forest domain {DomainName}", domain.Name);
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

        private static Domain GetDomain(string parsedDomain)
        {
            foreach (Domain domain in Forest.GetCurrentForest().Domains)
            {
                if(domain.Name.Contains(parsedDomain))
                    return domain;
            }
            return null;
        }
        /// <summary>
        /// Used to get the UserPrincpal based on username - will try the domain that are part of the username if present
        /// </summary>
        /// <param name="username">UPN or SamAccountName</param>
        /// <returns>Userprincipal if found else null</returns>
        public static UserPrincipal GetUserPrincipal(string username)
        {
            var parsedDomain = username.GetDomain();
            string strippedUsername = username.StripDomain();

            Domain matchedDomain = GetDomain(parsedDomain);
            // If a domain was present in the supplied username, try to find this first at validate against it.
            if (matchedDomain != null)
            {
                var user = GetUserPrincipal(matchedDomain, strippedUsername);
                if (user != null)
                    return user;
                Log.Warning("Null principal in domain: {DomainName}, user: {UserName}", matchedDomain.Name,
                    strippedUsername);
            }
            else
            {
                Log.Warning("Didn't GetDomain {parsedDomain}", parsedDomain);
            }

            foreach (Domain domain in Forest.GetCurrentForest().Domains)
            {
                Log.Information("Checking domain {DomainName}", domain);
                var user = GetUserPrincipal(domain, strippedUsername);
                if ( user != null)
                    return user;
                Log.Warning("Null principal in domain: {DomainName}, user: {UserName}", domain.Name, strippedUsername);
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
            foreach (Domain domain in Forest.GetCurrentForest().Domains)
            {
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
            foreach (Domain domain in Forest.GetCurrentForest().Domains)
            {
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