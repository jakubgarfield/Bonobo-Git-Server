using System.Configuration;
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace Bonobo.Git.Server.Configs
{
    public class ConfigEntry<Entry> where Entry : ConfigEntry<Entry>
    {
        protected ConfigEntry() { }

        private static Entry _current = null;
        private static readonly object _asyncRoot = new object();
        private static readonly string _configPath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["UserConfiguration"]);

        public static Entry Current { get { return _current ?? Load(); } }

        private static Entry Load()
        {
            if (_current == null)
                lock (_asyncRoot)
                    if (_current == null)
                    {
                        var xs = new XmlSerializer(typeof(Entry));
                        try
                        {
                            using (var stream = File.Open(_configPath, FileMode.Open))
                            {
                                _current = xs.Deserialize(stream) as Entry;
                            }
                        }
                        catch { }
                    }

            return _current;
        }

        public void Save()
        {
            if (_current != null)
                lock (_asyncRoot)
                    if (_current != null)
                    {
                        var xs = new XmlSerializer(typeof(Entry));
                        using (var stream = File.Open(_configPath, FileMode.Create))
                            xs.Serialize(stream, _current);
                    }
        }
    }
}