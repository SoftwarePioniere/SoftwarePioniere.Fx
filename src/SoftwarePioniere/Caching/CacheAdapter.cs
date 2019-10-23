using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Foundatio.Caching;
using Foundatio.Lock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Caching
{
    public class CacheAdapter : ICacheAdapter
    {
        private readonly ILockProvider _lockProvider;
        private readonly IEntityStore _entityStore;
        private readonly ILogger _logger;
        private readonly int _cacheMinutes;
        private readonly int _cacheSplitSize;

        public CacheAdapter(ILoggerFactory loggerFactory, ILockProvider lockProvider, ICacheClient cacheClient, IOptions<CacheOptions> options, IEntityStore entityStore)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _cacheMinutes = options.Value.CacheMinutes;
            _cacheSplitSize = options.Value.CacheLoadSplitSize;


            _lockProvider = lockProvider ?? throw new ArgumentNullException(nameof(lockProvider));
            _entityStore = entityStore ?? throw new ArgumentNullException(nameof(entityStore));
            CacheClient = cacheClient ?? throw new ArgumentNullException(nameof(cacheClient));
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public Task<bool> AddAsync<T>(string key, T value)
        {
            return CacheClient.SetAsync(key, value, TimeSpan.FromMinutes(60));
        }

        private static IEnumerable<IEnumerable<T>> Split<T>(T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }

        public async Task<List<T>> LoadSetItems<T>(string setKey, Expression<Func<T, bool>> @where, int minutes = int.MinValue, ILogger logger = null) where T : Entity
        {
            if (logger == null)
                logger = _logger;

            //gibt es die ALLE-Ids Liste?

            var existsAsync = await CacheClient.ExistsAsync(setKey);

            var items = new List<T>();

            //wenn nein, lade alle objekte
            if (!existsAsync)
            {
                logger.LogDebug("LoadSetItems: Key not Found in Cache {Key}", setKey);
                await _lockProvider.TryUsingAsync(setKey, async () =>
                {
                    var entities = await LoadListAndAddSetToCache(setKey, @where, minutes, logger);
                    items.AddRange(entities);
                }, timeUntilExpires: null, acquireTimeout: null);

                return items;
            }

            var idsListValue = await CacheClient.GetSetAsync<string>(setKey);
            if (idsListValue.HasValue)
            {
                var idList = idsListValue.Value;

                if (IsEmptyList(idList))
                    return items;

                var cacheValues = await CacheClient.GetAllAsync<T>(idList);

                //die im cache sind einfügen
                items.AddRange(cacheValues.Values.Where(x => x.HasValue).Select(x => x.Value));

                if (cacheValues.Any(x => !x.Value.HasValue))
                {
                    var loadedIds = items.Select(x => x.EntityId).ToArray();
                    //die anderen nachladen und einfügen
                    var allMissingIds = idList.Where(id => !loadedIds.Contains(id)).ToArray();

                    foreach (var missingIds in Split(allMissingIds, _cacheSplitSize))
                    {
                        //var alleEntities = await _entityStore.LoadItemsAsync<T>();
                        //var entities = alleEntities.Where(x => missingIds.Contains(x.EntityId)).ToList();

                        var entities = await _entityStore.LoadItemsAsync<T>(x => missingIds.Contains(x.EntityId));

                        foreach (var item in entities)
                        {
                            if (minutes == int.MinValue)
                                await CacheClient.AddAsync(item.EntityId, item, TimeSpan.FromMinutes(_cacheMinutes));
                            else
                                await CacheClient.AddAsync(item.EntityId, item, TimeSpan.FromMinutes(minutes));
                        }
                        
                        items.AddRange(entities);
                    }
                }

                return items;
            }

            logger.LogDebug("LoadSetItems: Key not Found in Cache {Key}", setKey);
            await _lockProvider.TryUsingAsync(setKey, async () =>
            {
                var entities = await LoadListAndAddSetToCache(setKey, @where, minutes, logger);
                items.AddRange(entities);
            }, timeUntilExpires: null, acquireTimeout: null);

            return items;


        }

        private const string EmptyKey = "EMPTY-0001111";
        private static readonly string[] EmptyList = { EmptyKey };

        private static bool IsEmptyList(ICollection<string> list)
        {
            return list != null && list.Count == 1 && list.Contains(EmptyKey);
        }

        public async Task<T[]> LoadListAndAddSetToCache<T>(string setKey, Expression<Func<T, bool>> @where, int minutes = int.MinValue, ILogger logger = null) where T : Entity
        {
            if (logger == null)
                logger = _logger;

            logger.LogDebug("LoadListAndAddSetToCache {setKey}", setKey);

            var entities = await _entityStore.LoadItemsAsync(@where);

            if (entities.Length > 0)
            {
                foreach (var item in entities)
                {
                    if (minutes == int.MinValue)
                        await CacheClient.AddAsync(item.EntityId, item, TimeSpan.FromMinutes(_cacheMinutes));
                    else
                        await CacheClient.AddAsync(item.EntityId, item, TimeSpan.FromMinutes(minutes));
                }

                var entityIds = entities.Select(x => x.EntityId).ToArray();
                if (minutes == int.MinValue)
                    await CacheClient.SetAddAsync(setKey, entityIds, TimeSpan.FromMinutes(_cacheMinutes));
                else
                    await CacheClient.SetAddAsync(setKey, entityIds, TimeSpan.FromMinutes(minutes));
            }
            else
            {
                //dummy set if empty list
                if (minutes == int.MinValue)
                    await CacheClient.SetAddAsync(setKey, EmptyList, TimeSpan.FromMinutes(_cacheMinutes));
                else
                    await CacheClient.SetAddAsync(setKey, EmptyList, TimeSpan.FromMinutes(minutes));
            }

            return entities;
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

        public ICacheClient CacheClient { get; }

        public Task<T> CacheLoad<T>(Func<Task<T>> loader, string cacheKey, int minutes = int.MinValue,
            ILogger logger = null)
        {
            return CacheLoadItem(loader, cacheKey, minutes, logger);

            //if (logger == null)
            //{
            //    logger = _logger;
            //}

            //if (await CacheClient.ExistsAsync(cacheKey))
            //{
            //    logger.LogTrace("Cache Key exists {CacheKey}", cacheKey);

            //    var l = await CacheClient.GetAsync<T>(cacheKey);
            //    if (l.HasValue)
            //    {
            //        logger.LogTrace("Return result from Cache with {CacheKey}", cacheKey);
            //        return l.Value;
            //    }
            //}

            //logger.LogTrace("No Cache Result {CacheKey}", cacheKey);

            //var ret = await loader();

            //if (ret != null)
            //{
            //    if (await IsPrefixLocked(cacheKey))
            //    {
            //        logger.LogTrace("Cache Key is Locked, Skipping add");
            //    }
            //    else
            //    {
            //        logger.LogTrace("Loaded Result. Set to Cache {CacheKey}", cacheKey);
            //        await CacheClient.SetAsync(cacheKey, ret, TimeSpan.FromMinutes(minutes));
            //    }
            //}

            //return ret;
        }

        public async Task<T> CacheLoadItem<T>(Func<Task<T>> loader, string cacheKey, int minutes = int.MinValue,
            ILogger logger = null)
        {
            if (logger == null)
            {
                logger = _logger;
            }

            logger.LogTrace("CacheLoad for EntityType: {EntityType} with Key {CacheKey}", typeof(T), cacheKey);

            if (await CacheClient.ExistsAsync(cacheKey))
            {
                logger.LogTrace("Cache Key exists {CacheKey}", cacheKey);

                var l = await CacheClient.GetAsync<T>(cacheKey);
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
                    if (minutes == int.MinValue)
                        await CacheClient.SetAsync(cacheKey, ret, TimeSpan.FromMinutes(_cacheMinutes));
                    else
                        await CacheClient.SetAsync(cacheKey, ret, TimeSpan.FromMinutes(minutes));


                    await CacheClient.SetAsync(cacheKey, ret, TimeSpan.FromMinutes(minutes));
                }
            }

            return ret;
        }

        public async Task<T[]> CacheLoadItems<T>(Func<Task<T[]>> loader, string cacheKey, int minutes = int.MinValue,
            ILogger logger = null)
        {
            if (logger == null)
            {
                logger = _logger;
            }

            logger.LogDebug("CacheLoad for EntityType: {EntityType} with Key {CacheKey}", typeof(T), cacheKey);

            if (await CacheClient.ExistsAsync(cacheKey))
            {
                logger.LogDebug("Cache Key {CacheKey} exists", cacheKey);

                var l = await CacheClient.GetSetAsync<T>(cacheKey);
                if (l.HasValue)
                {
                    logger.LogDebug("Return result from Cache");

                    return l.Value.ToArray();
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
                    if (minutes == int.MinValue)
                        await CacheClient.SetAddAsync(cacheKey, ret, TimeSpan.FromMinutes(_cacheMinutes));
                    else
                        await CacheClient.SetAddAsync(cacheKey, ret, TimeSpan.FromMinutes(minutes));
                }
            }

            return ret;
        }

        //public async Task<PagedResults<T>> CacheLoadPagedItems<T>(Func<Task<PagedResults<T>>> loader, string cacheKey,
        //    int minutes = 60, ILogger logger = null)
        //{
        //    if (logger == null)
        //    {
        //        logger = _logger;
        //    }


        //    logger.LogDebug("CacheLoad for EntityType: {EntityType} with Key {CacheKey}", typeof(T), cacheKey);

        //    if (await CacheClient.ExistsAsync(cacheKey))
        //    {
        //        logger.LogDebug("Cache Key {CacheKey} exists", cacheKey);

        //        var l = await CacheClient.GetAsync<PagedResults<T>>(cacheKey);
        //        if (l.HasValue)
        //        {
        //            logger.LogDebug("Return result from Cache");

        //            return l.Value;
        //        }
        //    }

        //    logger.LogDebug("No Cache Result. Loading and Set to Cache");

        //    var ret = await loader();
        //    if (ret != null && ret.ResultCount > 0)
        //    {
        //        if (await IsPrefixLocked(cacheKey))
        //        {
        //            logger.LogTrace("Cache Key is Locked, Skipping add");
        //        }
        //        else
        //        {
        //            await CacheClient.SetAsync(cacheKey, ret, DateTime.UtcNow.AddMinutes(minutes));
        //        }
        //    }

        //    return ret;
        //}

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
            return CacheClient.RemoveByPrefixAsync(prefix);
        }
    }
}