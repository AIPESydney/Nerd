using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using Ninject;
using Nerd.Api.Interfaces;

namespace Nerd.Api.Providers
{
    public class RuntimeCacheProvider : ICacheProvider
    {

        private static readonly ConcurrentDictionary<Type, string> Keys = new ConcurrentDictionary<Type, string>();
        private static readonly MemoryCache Cache = MemoryCache.Default;


        private string _keyPrefix;
        public string KeyPrefix
        {
            get { return _keyPrefix ?? (_keyPrefix = "nd"); }
        }

        public T Get<T>()
        {
            var key = GetKey<T>();
            return Cache[key] is T ? (T)Cache[key] : default(T);
        }

        public void Save<T>(T item, int cacheInMins = 0)
        {
            var key = GetKey<T>();
            Cache.Add(key,
                      item,
                      new CacheItemPolicy
                      {
                          AbsoluteExpiration = DateTime.Now.AddMinutes(cacheInMins),
                          SlidingExpiration = TimeSpan.Zero,
                          Priority = CacheItemPriority.Default
                      });
        }

        public void Clear<T>()
        {
            var key = GetKey<T>();
            Cache.Remove(key);
        }

        private string GetKey<T>()
        {
            string key;
            var type = typeof(T);

            if (!Keys.TryGetValue(type, out key))
            {
                key = string.Format("{0}{1}", string.IsNullOrWhiteSpace(KeyPrefix) ? string.Empty : KeyPrefix, typeof(T).FullName);
                Keys.TryAdd(typeof(T), key);
            }

            return key;
        }
    }
}