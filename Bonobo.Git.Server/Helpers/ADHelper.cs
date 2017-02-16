using Bonobo.Git.Server.Configuration;
using System;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;

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
            var parsedDomain = username.GetDomain();
            string strippedUsername = username.StripDomain();

            Domain matchedDomain = GetDomain(parsedDomain);
            // If a domain was present in the supplied username, try to find this first and validate against it.
            if(matchedDomain != null)
            {
                return ValidateUser(matchedDomain, strippedUsername, password);
            }
            // Else try all domains in the current forest.
            foreach (Domain domain in Forest.GetCurrentForest().Domains)
            {
                if (ValidateUser(matchedDomain, strippedUsername, password))
                    return true;
            }

            return false;
        }

        private static bool ValidateUser(Domain domain, string username, string password)
        {
            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domain.Name))
                {
                    if (pc.ValidateCredentials(username, password, ContextOptions.Negotiate))
                        return true;
                }
            }
            catch (Exception exp)
            {
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
        /// <param name="username">UPN or sAMAccountName</param>
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
            }

            foreach (Domain domain in Forest.GetCurrentForest().Domains)
            {
                var user = GetUserPrincipal(domain, strippedUsername);
                if( user != null)
                    return user;              
            }

            return null;
        }

        private static UserPrincipal GetUserPrincipal(Domain domain, string username)
        {
            try
            {
                using (var pc = new PrincipalContext(ContextType.Domain, domain.Name))
                {
                    var user = UserPrincipal.FindByIdentity(pc, username);
                    if (user != null)
                        return user;
                }
            }
            catch (Exception exp)
            {
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
                    Trace.TraceError(exp.Message);
                    if (exp.InnerException != null)
                        Trace.TraceError(exp.InnerException.Message);
                    // let it fail
                }
            }
            throw new ArgumentException("Could not find principal group: " + name);
        }
    }
}