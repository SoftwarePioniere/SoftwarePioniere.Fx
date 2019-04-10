using System;
using System.Threading.Tasks;
using Foundatio.Caching;
using Foundatio.Lock;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Caching.Services
{
    public class CacheAdapter : ICacheAdapter
    {
        private readonly ICacheClient _cache;
        private readonly ILockProvider _lockProvider;
        private readonly ILogger _logger;
        
        public CacheAdapter(ILoggerFactory loggerFactory, ICacheClient cache, ILockProvider lockProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _lockProvider = lockProvider ?? throw new ArgumentNullException(nameof(lockProvider));
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public Task<bool> AddAsync<T>(string key, T value)
        {
            return _cache.AddAsync(key, value);
        }

        private async Task<bool> IsPrefixLocked(string cacheKey)
        {
            var splits = cacheKey.Split('.');
            if (splits.Length > 0)
            {
                var prefix = splits[0];
                return await _lockProvider.IsLockedAsync($"CACHE-{prefix}");
            }

            return false;
        }

        public async Task<T> CacheLoad<T>(Func<Task<T>> loader, string cacheKey, int minutes = 60,
            ILogger logger = null)
        {
            if (logger == null)
            {
                logger = _logger;
            }

            if (await _cache.ExistsAsync(cacheKey))
            {
                logger.LogTrace("Cache Key exists {CacheKey}", cacheKey);

                var l = await _cache.GetAsync<T>(cacheKey);
                if (l.HasValue)
                {
                    logger.LogTrace("Return result from Cache with {CacheKey}", cacheKey);
                    return l.Value;
                }
            }

            logger.LogTrace("No Cache Result {CacheKey}", cacheKey);

            var ret = await loader();

            if (ret != null)
            {
                if (await IsPrefixLocked(cacheKey))
                {
                    logger.LogTrace("Cache Key is Locked, Skipping add");
                }
                else
                {
                    logger.LogTrace("Loaded Result. Set to Cache {CacheKey}", cacheKey);
                    await _cache.SetAsync(cacheKey, ret, DateTime.UtcNow.AddMinutes(minutes));
                }
            }

            return ret;
        }

        public async Task<T> CacheLoadItem<T>(Func<Task<T>> loader, string cacheKey, int minutes = 60,
            ILogger logger = null)
        {
            if (logger == null)
            {
                logger = _logger;
            }

            logger.LogTrace("CacheLoad for EntityType: {EntityType} with Key {CacheKey}", typeof(T), cacheKey);

            if (await _cache.ExistsAsync(cacheKey))
            {
                logger.LogTrace("Cache Key exists {CacheKey}", cacheKey);

                var l = await _cache.GetAsync<T>(cacheKey);
                if (l.HasValue)
                {
                    logger.LogTrace("Return result from Cache with {CacheKey}", cacheKey);
                    return l.Value;
                }
            }

            logger.LogTrace("No Cache Result {CacheKey}", cacheKey);

            var ret = await loader();
            if (ret != null)
            {
                if (await IsPrefixLocked(cacheKey))
                {
                    logger.LogTrace("Cache Key is Locked, Skipping add");
                }
                else
                {
                    await _cache.SetAsync(cacheKey, ret, DateTime.UtcNow.AddMinutes(minutes));
                }
            }

            return ret;
        }

        public async Task<T[]> CacheLoadItems<T>(Func<Task<T[]>> loader, string cacheKey, int minutes = 60,
            ILogger logger = null)
        {
            if (logger == null)
            {
                logger = _logger;
            }

            logger.LogDebug("CacheLoad for EntityType: {EntityType} with Key {CacheKey}", typeof(T), cacheKey);

            if (await _cache.ExistsAsync(cacheKey))
            {
                logger.LogDebug("Cache Key {CacheKey} exists", cacheKey);

                var l = await _cache.GetAsync<T[]>(cacheKey);
                if (l.HasValue)
                {
                    logger.LogDebug("Return result from Cache");

                    return l.Value;
                }
            }

            logger.LogDebug("No Cache Result. Loading and Set to Cache");

            var ret = await loader();
            if (ret != null && ret.Length > 0)
            {
                if (await IsPrefixLocked(cacheKey))
                {
                    logger.LogTrace("Cache Key is Locked, Skipping add");
                }
                else
                {
                    await _cache.SetAsync(cacheKey, ret, DateTime.UtcNow.AddMinutes(minutes));
                }
            }

            return ret;
        }

        public async Task<PagedResults<T>> CacheLoadPagedItems<T>(Func<Task<PagedResults<T>>> loader, string cacheKey,
            int minutes = 60, ILogger logger = null)
        {
            if (logger == null)
            {
                logger = _logger;
            }


            logger.LogDebug("CacheLoad for EntityType: {EntityType} with Key {CacheKey}", typeof(T), cacheKey);

            if (await _cache.ExistsAsync(cacheKey))
            {
                logger.LogDebug("Cache Key {CacheKey} exists", cacheKey);

                var l = await _cache.GetAsync<PagedResults<T>>(cacheKey);
                if (l.HasValue)
                {
                    logger.LogDebug("Return result from Cache");

                    return l.Value;
                }
            }

            logger.LogDebug("No Cache Result. Loading and Set to Cache");

            var ret = await loader();
            if (ret != null && ret.ResultCount > 0)
            {
                if (await IsPrefixLocked(cacheKey))
                {
                    logger.LogTrace("Cache Key is Locked, Skipping add");
                }
                else
                {
                    await _cache.SetAsync(cacheKey, ret, DateTime.UtcNow.AddMinutes(minutes));
                }
            }

            return ret;
        }

        //public async Task LockPrefix(string prefix)
        //{
        //    await _lockProvider.TryUsingAsync(LockyKey,
        //        async token => { await _cache.SetAsync($"LOCK-{prefix}", true); },
        //        cancellationToken: CancellationToken.None);
        //}

        //public async Task ReleasePrefix(string prefix)
        //{
        //    await _lockProvider.TryUsingAsync(LockyKey,
        //        async token => { await _cache.RemoveAsync($"LOCK-{prefix}"); },
        //        cancellationToken: CancellationToken.None);
        //}

        public Task<int> RemoveByPrefixAsync(string prefix)
        {
            return _cache.RemoveByPrefixAsync(prefix);
        }
    }
}