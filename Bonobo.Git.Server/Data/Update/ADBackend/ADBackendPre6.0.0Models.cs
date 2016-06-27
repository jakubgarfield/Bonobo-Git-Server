
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Bonobo.Git.Server.Data.Update.Pre600ADBackend
{
    public interface Pre600INameProperty
    {
        string Name { get; }
    }

    // These models were used pre 6.0.0
    public class Pre600RoleModel : Pre600INameProperty
    {
        public string Name { get; set; }
        public string[] Members { get; set; }
    }

    public class Pre600UserModel : Pre600INameProperty
    {
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

        public string DisplayName
        {
            get
            {
                return String.Format("{0} {1}", GivenName, Surname);
            }
        }
    }

    public class Pre600TeamModel : Pre600INameProperty
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Members { get; set; }
    }


    public class Pre600RepositoryModel : Pre600INameProperty
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }
        public bool AnonymousAccess { get; set; }
        public string[] Users { get; set; }
        public string[] Administrators { get; set; }
        public string[] Teams { get; set; }
        public bool AuditPushUser { get; set; }
        public byte[] Logo { get; set; }
        public bool RemoveLogo { get; set; }
    }

    public class Pre600Functions
    {
        public static ConcurrentDictionary<string, T> LoadContent<T>(string storagePath) where T : Pre600INameProperty
        {
            ConcurrentDictionary<string, T> result = new ConcurrentDictionary<string, T>();

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            foreach (string filename in Directory.EnumerateFileSystemEntries(storagePath, "*.json"))
            {
                try
                {
                    T item = JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
                    result.TryAdd(item.Name, item);
                }
                catch
                {
                }
            }

            return result;
        }
    }
}