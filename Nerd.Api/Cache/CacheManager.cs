using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Ninject;
using Nerd.Api.NinjectInterfaces;
using Nerd.Api.Interfaces;

namespace Nerd.Api.Cache
{
    public interface ICacheManager : ISingletonScope
    {
        Task<T> CacheAsync<T>(Func<Task<T>> func, int cacheInMins = CacheManager.DefaultCacheTimeInMins, bool reload = false) where T : class;
        Task<IEnumerable<T>> CachefromRegAsync<T>(int cacheInMins = CacheManager.DefaultCacheTimeInMins, bool reload = false) where T : class;
    }

    public class CacheManager : ICacheManager
    {
        public const int DefaultCacheTimeInMins = 10;
        public const int NoCache = 0;

        [Inject]
        public ICacheProvider CacheProvider { get; set; }

        //use Semaphore because it doesn't have thread affinity 
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private static readonly IDictionary<Type, SemaphoreSlim> KeySemaphores = new Dictionary<Type, SemaphoreSlim>();

        private static readonly object Locker = new { };
        private static readonly IList<Type> LoadingKeys = new List<Type>();

        public static readonly IDictionary<Type, Delegate> Registry = new Dictionary<Type, Delegate>();

        public T Fetch<T>(Func<T> func, int cacheInMins = DefaultCacheTimeInMins, bool reload = false) where T : class
        {
            if (CacheProvider == null || cacheInMins <= 0)
                return func();

            var key = typeof(T);
            var data = reload ? null : CacheProvider.Get<T>();

            if (data != null) return data;

            Type keyInUse;

            //lock for LoadingKeys to allow only one thread to update the list
            lock (Locker)
            {
                keyInUse = LoadingKeys.FirstOrDefault(x => key == x);
                if (keyInUse == null)
                {
                    //double check the cache in case other thread had just inserted the key after out first get from cache tempt and before we check the key exists in the loadingKeys.
                    data = CacheProvider.Get<T>();
                    if (data != null)
                        return data;
                    keyInUse = key;
                    key = null; // seting key to null as a flag to indicate the current thread put the key in the list.
                    LoadingKeys.Add(keyInUse);
                }
            }

            //lock the key to allow only one thread to load the value and save to the cache.
            //two locks are used to allow concurrent loading of different keys to improve performance.
            lock (keyInUse)
            {
                // check if the current thread is the first one locking the key (by putting the key in the list and setting key to null as a flag)
                if (key == null)
                {
                    try
                    {
                        data = func();

                        if (data != null)
                        {
                            CacheProvider.Save(data, cacheInMins);
                        }
                    }
                    finally // no matter what happened, the following needs to be done.
                    {
                        lock (Locker)
                        {
                            LoadingKeys.Remove(keyInUse);
                        }
                        Monitor.PulseAll(keyInUse);
                    }

                    if (data == null)
                        return null;
                }
                else if (LoadingKeys.Contains(keyInUse))
                    Monitor.Wait(keyInUse);
            }

            return data ?? CacheProvider.Get<T>();

        }

        public async Task<IEnumerable<T>> CachefromRegAsync<T>(int cacheInMins = DefaultCacheTimeInMins, bool reload = false) where T : class
        {
            var type = typeof(T);
            var func = Registry[type] as Func<Task<IEnumerable<T>>>;

            if (func == null) return null;

            return await CacheAsync(func, cacheInMins, reload);
        }

        public async Task<T> CacheAsync<T>(Func<Task<T>> func, int cacheInMins = DefaultCacheTimeInMins, bool reload = false) where T : class
        {

            if (func == null)
                return null;

            if (CacheProvider == null || cacheInMins <= 0)
                return await func();

            var data = reload ? null : CacheProvider.Get<T>();

            if (data != null) return data;

            var key = typeof(T);
            SemaphoreSlim keySemaphore;

            //lock for KeySemaphores to allow only one thread to update the dictionary
            Semaphore.Wait();
            if (!KeySemaphores.TryGetValue(key, out keySemaphore))
            {
                //two locks are used to allow concurrent loading of different keys to improve performance.
                KeySemaphores[key] = keySemaphore = new SemaphoreSlim(1, 1);
                keySemaphore.Wait();
                // seting key to null as a flag to indicate the current thread should be the one to load data into cache.
                key = null;
            }
            Semaphore.Release();
            // check if the current thread should be the one loading data (key equals to null as a flag)
            if (key == null)
            {
                try
                {
                    data = await func();

                    if (data != null)
                    {
                        CacheProvider.Save(data, cacheInMins);
                    }
                }
                finally // no matter what happened, the following needs to be done.
                {
                    Semaphore.Wait();
                    KeySemaphores.Remove(typeof(T));
                    keySemaphore.Release();
                    keySemaphore = null;
                    Semaphore.Release();
                }

                if (data == null)
                    return null;
            }
            //another thread is loading the cache wait for it to finish.
            else if (keySemaphore != null)
                keySemaphore.Wait();

            if (keySemaphore != null)
                keySemaphore.Release();

            return data ?? CacheProvider.Get<T>();
        }

        public static void Register<T>(Func<Task<IEnumerable<T>>> func)
        {
            Registry.Add(typeof(T), func);
        }
    }
}