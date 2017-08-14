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
using System.Web.Hosting;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Helpers;
using Serilog;

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

        public ADBackendStore(string rootpath, string name)
        {
            _storagePath = Path.Combine(PathEncoder.GetRootPath(rootpath), name);
            _content = LoadContent();
        }

        public bool Add(T item)
        {
            if (item.Id == Guid.Empty)
            {
                throw new ArgumentException("You must set the Id before adding an item");
            }
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
            if (item.Id == Guid.Empty)
            {
                throw new ArgumentException("Item does not have a proper Id");
            }

            bool result = false;

            try
            {
                string itemFilename = Path.Combine(_storagePath, GetItemFilename(item));
                File.WriteAllText(itemFilename, JsonConvert.SerializeObject(item));
                result = true;
            }
            catch(Exception ex)
            {
                Log.Error(ex, "AD: Store");
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
                Log.Error(ex, "AD: Delete");
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
                    Log.Error(ex, "AD: LoadContent");
                }
            }

            return result;
        }

        private string GetItemFilename(T item)
        {
            return item.Id+".json";
        }
    }
}