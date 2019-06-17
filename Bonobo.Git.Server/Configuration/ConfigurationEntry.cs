using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using Bonobo.Git.Server.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace Bonobo.Git.Server.Configuration
{
    public abstract class ConfigurationEntry<Entry> where Entry : ConfigurationEntry<Entry>, new()
    {
        private static Entry _current = null;
        protected static IHostingEnvironment hostingEnvironment;
        private static readonly object _sync = new object();
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(Entry));
        private static string ConfigPath => Path.IsPathRooted(ConfigurationManager.AppSettings["UserConfiguration"])
            ? ConfigurationManager.AppSettings["UserConfiguration"]
            : hostingEnvironment.MapPath(ConfigurationManager.AppSettings["UserConfiguration"]);

        public static Entry Current { get { return _current ?? Load(); } }


        private static Entry Load()
        {
            lock (_sync)
            {
                if (_current == null)
                {
                    try
                    {
                        using (var stream = File.Open(ConfigPath, FileMode.Open))
                        {
                            _current = _serializer.Deserialize(stream) as Entry;
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        _current = new Entry();
                    }
                }
            }

            return _current;
        }

        public void Save()
        {
            lock (_sync)
            {
                if (_current != null)
                {
                    using (var stream = File.Open(ConfigPath, FileMode.Create))
                    {
                        _serializer.Serialize(stream, _current);
                    }
                }
            }
        }

        public static void InitialiseForTest()
        {
            _current = new Entry();
        }
    }
}