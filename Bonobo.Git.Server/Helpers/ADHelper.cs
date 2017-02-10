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
        public static bool ValidateUser(string username, string password)
        {
            foreach (Domain domain in Forest.GetCurrentForest().Domains)
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
                    // let it fail
                }
            }

            return false;
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