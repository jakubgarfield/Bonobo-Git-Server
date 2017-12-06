using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

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

            if (handshakeCache.TryGetValue(key, out object cachedHandshake))
            {
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
            handshakeCache.Remove(key);
            return true;
        }

        private static MemoryCacheEntryOptions GetCacheItemPolicy()
        {
            var policy = new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.Normal,
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(expirationTimeInMinutes),
            };

            policy.RegisterPostEvictionCallback((key, value, reason, state) =>
                 {
                     IDisposable expiredHandshake = value as IDisposable;
                     if (expiredHandshake != null)
                     {
                         expiredHandshake.Dispose();
                     }
                 });

            return policy;
        }

        public WindowsAuthenticationHandshakeCache(string name)
        {
            handshakeCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        }
    }
}