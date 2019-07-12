//using System;
//using System.Threading.Tasks;
//using Foundatio.Caching;
//using Microsoft.Extensions.Logging;
//using SoftwarePioniere.ReadModel;

//// ReSharper disable CheckNamespace

//namespace Fliegel365
//{

//    public static class CacheClientExtensions
//    {

//        public static async Task<T> CacheLoad<T>(this ICacheClient cache, Func<Task<T>> loader, string cacheKey,
//            int minutes = 60, ILogger logger = null)
//        {
//            if (await cache.ExistsAsync(cacheKey))
//            {
//                logger?.LogTrace("Cache Key exists {CacheKey}", cacheKey);

//                var l = await cache.GetAsync<T>(cacheKey);
//                if (l.HasValue)
//                {
//                    logger?.LogTrace("Return result from Cache with {CacheKey}", cacheKey);

//                    return l.Value;
//                }
//            }

//            logger?.LogTrace("No Cache Result {CacheKey}", cacheKey);

//            var ret = await loader();

//            if (ret != null)
//            {
//                logger?.LogTrace("Loaded Result. Set to Cache {CacheKey}", cacheKey);
//                await cache.SetAsync(cacheKey, ret, DateTime.UtcNow.AddMinutes(minutes));

//            }

//            return ret;
//        }

//        public static async Task<T> CacheLoadItem<T>(this ICacheClient cache, Func<Task<T>> loader, string cacheKey,
//         int minutes = 60, ILogger logger = null)
//        {
//            logger?.LogTrace("CacheLoad for EntityType: {EntityType} with Key {CacheKey}", typeof(T), cacheKey);

//            if (await cache.ExistsAsync(cacheKey))
//            {
//                logger?.LogTrace("Cache Key exists {CacheKey}", cacheKey);

//                var l = await cache.GetAsync<T>(cacheKey);
//                if (l.HasValue)
//                {
//                    logger?.LogTrace("Return result from Cache with {CacheKey}", cacheKey);
//                    return l.Value;
//                }
//            }

//            logger?.LogTrace("No Cache Result {CacheKey}", cacheKey);

//            var ret = await loader();
//            if (ret != null)
//                await cache.SetAsync(cacheKey, ret, DateTime.UtcNow.AddMinutes(minutes));

//            return ret;
//        }


//        public static async Task<T[]> CacheLoadItems<T>(this ICacheClient cache, Func<Task<T[]>> loader, string cacheKey,
//            int minutes = 60, ILogger logger = null)
//        {
//            if (logger != null && logger.IsEnabled(LogLevel.Debug))
//                logger.LogDebug("CacheLoad for EntityType: {EntityType} with Key {CacheKey}", typeof(T), cacheKey);

//            if (await cache.ExistsAsync(cacheKey))
//            {
//                if (logger != null && logger.IsEnabled(LogLevel.Debug))
//                    logger.LogDebug("Cache Key {CacheKey} exists", cacheKey);


//                var l = await cache.GetAsync<T[]>(cacheKey);
//                if (l.HasValue)
//                {
//                    if (logger != null && logger.IsEnabled(LogLevel.Debug))
//                        logger.LogDebug("Return result from Cache");

//                    return l.Value;
//                }
//            }

//            if (logger != null && logger.IsEnabled(LogLevel.Debug))
//                logger.LogDebug("No Cache Result. Loading and Set to Cache");

//            var ret = await loader();
//            if (ret != null && ret.Length > 0)
//            {
//                await cache.SetAsync(cacheKey, ret, DateTime.UtcNow.AddMinutes(minutes));
//            }

//            return ret;
//        }


//        public static async Task<PagedResults<T>> CacheLoadPagedItems<T>(this ICacheClient cache, Func<Task<PagedResults<T>>> loader, string cacheKey,
//            int minutes = 60, ILogger logger = null)
//        {
//            if (logger != null && logger.IsEnabled(LogLevel.Debug))
//                logger.LogDebug("CacheLoad for EntityType: {EntityType} with Key {CacheKey}", typeof(T), cacheKey);

//            if (await cache.ExistsAsync(cacheKey))
//            {
//                if (logger != null && logger.IsEnabled(LogLevel.Debug))
//                    logger.LogDebug("Cache Key {CacheKey} exists", cacheKey);


//                var l = await cache.GetAsync<PagedResults<T>>(cacheKey);
//                if (l.HasValue)
//                {
//                    if (logger != null && logger.IsEnabled(LogLevel.Debug))
//                        logger.LogDebug("Return result from Cache");

//                    return l.Value;
//                }
//            }

//            if (logger != null && logger.IsEnabled(LogLevel.Debug))
//                logger.LogDebug("No Cache Result. Loading and Set to Cache");

//            var ret = await loader();
//            if (ret != null && ret.ResultCount > 0)
//            {
//                await cache.SetAsync(cacheKey, ret, DateTime.UtcNow.AddMinutes(minutes));
//            }

//            return ret;
//        }
//    }
//}
