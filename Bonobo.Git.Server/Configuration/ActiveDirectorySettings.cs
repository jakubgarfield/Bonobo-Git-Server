using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Configuration
{
    public class ActiveDirectorySettings
    {
        public static string DefaultDomain { get; private set; }
        public static string MemberGroupName { get; private set; }
        public static string BackendPath { get; private set; }
        public static IDictionary<string, string> RoleNameToGroupNameMapping { get; private set; }
        public static IDictionary<string, string> TeamNameToGroupNameMapping { get; private set; }

        private static IDictionary<string, string> CreateMapping(string definition)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (!String.IsNullOrEmpty(definition))
            {
                foreach (string entry in definition.Split(',', ';'))
                {
                    string[] mapping = entry.Split('=');
                    if (mapping.Length == 2)
                    {
                        string key = mapping[0].Trim();
                        string value = mapping[1].Trim();
                        if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(value))
                        {
                            result.Add(key, value);
                        }
                    }
                }
            }

            return result;
        }

        static ActiveDirectorySettings()
        {
            DefaultDomain = ConfigurationManager.AppSettings["ActiveDirectoryDefaultDomain"];
            MemberGroupName = ConfigurationManager.AppSettings["ActiveDirectoryMemberGroupName"];
            BackendPath = ConfigurationManager.AppSettings["ActiveDirectoryBackendPath"];

            RoleNameToGroupNameMapping = CreateMapping(ConfigurationManager.AppSettings["ActiveDirectoryRoleMapping"]);
            TeamNameToGroupNameMapping = CreateMapping(ConfigurationManager.AppSettings["ActiveDirectoryTeamMapping"]);
        }
    }
}