using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonobo.Git.Server.Test.IntegrationTests
{
    public class LoadedConfig
    {

        private Dictionary<string, Dictionary<string, string>> _creds;

        public LoadedConfig(Dictionary<string, Dictionary<string, string>> creds)
        {
            _creds = creds;
                // tc.Properties["AdminCredentials"] = string.Format("{0}:{1}@", creds["admin"]["username"], creds["admin"]["password"]);
                // tc.Properties["UserCredentials"] = string.Format("{0}:{1}@", creds["user"]["username"], creds["user"]["password"]);
                // tc.Properties["RepositoryUrlWithCredentials"] = String.Format(RepositoryUrlTemplate, AdminCredentials, ".git", RepositoryName);
        }

        public Tuple<string, string> getCredentials(string who)
        {
            return new Tuple<string, string>(_creds[who]["username"], _creds[who]["password"]);
        }

        public string getUrlLogin(string who)
        {
            var ac = getCredentials(who);
            return string.Format("{0}:{1}", ac.Item1, ac.Item2);
        }
    }
}
