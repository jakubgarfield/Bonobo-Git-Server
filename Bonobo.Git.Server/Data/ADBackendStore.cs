using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Collections;
using System.Diagnostics;
using System.Web.Hosting;
using Bonobo.Git.Server.Configuration;

namespace Bonobo.Git.Server.Data
{
    public class ADBackendStore<T> : IEnumerable<T> where T : INameProperty
    {
        public T this[Guid key]
        {
            get
            {
                T result = default(T);

                _content.TryGetValue(key, out result);

                return result;
            }

            set
            {
                _content.AddOrUpdate(key, value, (k, v) => value);
            }
        }

        private readonly string _storagePath;
        private readonly ConcurrentDictionary<Guid, T> _content;
        private readonly string hexchars = "0123456789abcdef";

        public ADBackendStore(string rootpath, string name)
        {
            _storagePath = Path.Combine(GetRootPath(rootpath), name);
            _content = LoadContent();
        }

        public bool Add(T item)
        {
            return _content.TryAdd(item.Id, item) && Store(item);
        }

        public bool Remove(Guid key)
        {
            T removedItem;
            return _content.TryRemove(key, out removedItem) && Delete(removedItem);
        }

        public bool Remove(T item)
        {
            return Remove(item.Id);
        }

        public void Update(T item)
        {
            if (_content.TryUpdate(item.Id, item, _content[item.Id]))
            {
                Store(item);
            }
        }

        public void AddOrUpdate(T item)
        {
            _content.AddOrUpdate(item.Id, item, (k, v) => item);
            Store(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _content.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private bool Store(T item)
        {
            bool result = false;

            try
            {
                string itemFilename = Path.Combine(_storagePath, GetItemFilename(item));
                File.WriteAllText(itemFilename, JsonConvert.SerializeObject(item));
                result = true;
            }
            catch(Exception ex)
            {
                Trace.TraceError("ADStoreErr: " + ex);
            }

            return result;
        }

        private bool Delete(T item)
        {
            bool result = false;

            try
            {
                string itemFilename = Path.Combine(_storagePath, GetItemFilename(item));
                File.Delete(itemFilename);
                result = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("ADStoreErr: " + ex);
            }

            return result;
        }

        private ConcurrentDictionary<Guid, T> LoadContent()
        {
            var result = new ConcurrentDictionary<Guid, T>();

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

            foreach (string filename in Directory.EnumerateFileSystemEntries(_storagePath, "*.json"))
            {
                try
                {
                    T item = JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
                    result.TryAdd(item.Id, item);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("ADStoreErr: " + ex);
                }
            }

            return result;
        }

        private string GetRootPath(string path)
        {
            return Path.IsPathRooted(path) ? path : HostingEnvironment.MapPath(path);
        }

        private string GetItemFilename(T item)
        {
            StringBuilder result = new StringBuilder(45);

            byte[] hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(item.Name));
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hexchars[hash[i] >> 4]);
                result.Append(hexchars[hash[i] & 0x0f]);
            }
            result.Append(".json");

            return result.ToString();
        }
    }
}