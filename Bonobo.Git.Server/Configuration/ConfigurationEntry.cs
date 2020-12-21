using System.IO;
using System.Xml.Serialization;

namespace Bonobo.Git.Server.Configuration
{
    public abstract class ConfigurationEntry<Entry> where Entry : ConfigurationEntry<Entry>, new()
    {
        private static Entry _current = null;
        private static IPathResolver pathResolver = new HostingEnvironmentPathResolver();
        private static readonly object _sync = new object();
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(Entry));
        public static IPathResolver PathResolver { get => pathResolver; set => pathResolver = value; }
        private static string ConfigPath { get => PathResolver.ResolveWithConfiguration("UserConfiguration"); }


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