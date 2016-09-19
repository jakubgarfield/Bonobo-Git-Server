using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace Bonobo.Git.Server.Owin.Windows
{
    internal class WindowsAuthenticationHandshakeCache
    {
        private static readonly int expirationTimeInMinutes = 1;
        private MemoryCache handshakeCache;

        public bool TryGet(string key, out WindowsAuthenticationHandshake handshake)
        {
            bool result = false;
            handshake = null;

            if (handshakeCache.Contains(key))
            {
                object cachedHandshake = handshakeCache[key];
                if (cachedHandshake != null)
                {
                    handshake = cachedHandshake as WindowsAuthenticationHandshake; 
                    result = true;
                }
            }

            return result;
        }

        public void Add(string key, WindowsAuthenticationHandshake handshake)
        {
            handshakeCache.Set(key, handshake, GetCacheItemPolicy());
        }

        public bool TryRemove(string key)
        {
            return handshakeCache.Remove(key) != null;
        }

        private static CacheItemPolicy GetCacheItemPolicy()
        {
            var policy = new CacheItemPolicy()
            {
                Priority = CacheItemPriority.Default,
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(expirationTimeInMinutes),
                RemovedCallback = (handshake) =>
                {
                    IDisposable expiredHandshake = handshake.CacheItem as IDisposable;
                    if (expiredHandshake != null)
                    {
                        expiredHandshake.Dispose();
                    }
                }
            };
            return policy;
        }

        public WindowsAuthenticationHandshakeCache(string name)
        {
            handshakeCache = new MemoryCache(name);
        }
    }
}