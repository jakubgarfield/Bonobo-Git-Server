using System.Configuration;
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace Bonobo.Git.Server.Configuration
{
    public abstract class ConfigurationEntry<Entry> where Entry : ConfigurationEntry<Entry>, new()
    {
        private static Entry _current = null;
        private static readonly object _sync = new object();
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(Entry));
        private static readonly string _configPath = Path.IsPathRooted(ConfigurationManager.AppSettings["UserConfiguration"])
                                                    ? ConfigurationManager.AppSettings["UserConfiguration"]
                                                    : HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["UserConfiguration"]);


        public static Entry Current { get { return _current ?? Load(); } }


        private static Entry Load()
        {
            lock (_sync)
            {
                if (_current == null)
                {
                    try
                    {
                        using (var stream = File.Open(_configPath, FileMode.Open))
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
                    using (var stream = File.Open(_configPath, FileMode.Create))
                    {
                        _serializer.Serialize(stream, _current);
                    }
                }
            }
        }
    }
}