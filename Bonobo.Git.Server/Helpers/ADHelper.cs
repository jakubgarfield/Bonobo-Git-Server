using Bonobo.Git.Server.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Helpers
{
    public static class ADHelper
    {
        public static bool ValidateUser(string parsedDomain, string username, string password)
        {
            Domain matchedDomain = GetDomain(parsedDomain);
            // If a domain was present in the supplied username, try to find this first at validate against it.
            if(matchedDomain != null)
            {
                return ValidateUser(matchedDomain, username, password);
            }
            // Else try all domains in the current forest.
            foreach (Domain domain in Forest.GetCurrentForest().Domains)
            {
                if (ValidateUser(matchedDomain, username, password))
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

        public static UserPrincipal GetUserPrincipal(string username)
        {
            foreach (Domain domain in Forest.GetCurrentForest().Domains)
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
            }

            return null;
        }

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

        public static PrincipalContext GetMembersGroup(out GroupPrincipal group)
        {
            return GetPrincipalGroup(ActiveDirectorySettings.MemberGroupName, out group);
        }

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