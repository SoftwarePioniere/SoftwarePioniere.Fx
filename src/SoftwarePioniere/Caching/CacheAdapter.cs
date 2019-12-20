using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
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
        private const string EmptyKey = "EMPTY-0001111";
        private static readonly string[] EmptyList = { EmptyKey };
        private readonly IEntityStore _entityStore;
        private readonly ILockProvider _lockProvider;
        private readonly ILogger _logger;
        private readonly CacheOptions _options;

        public CacheAdapter(ILoggerFactory loggerFactory, ILockProvider lockProvider, ICacheClient cacheClient, IOptions<CacheOptions> options, IEntityStore entityStore)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));


            _options = options.Value;

            _lockProvider = lockProvider ?? throw new ArgumentNullException(nameof(lockProvider));
            _entityStore = entityStore ?? throw new ArgumentNullException(nameof(entityStore));
            CacheClient = cacheClient ?? throw new ArgumentNullException(nameof(cacheClient));
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public Task<bool> AddAsync<T>(string key, T value)
        {
            return CacheClient.SetAsync(key, value, TimeSpan.FromMinutes(60));
        }

        public async Task<List<T>> LoadSetItems<T>(string setKey, Expression<Func<T, bool>> where, int minutes = int.MinValue, CancellationToken cancellationToken = default) where T : Entity
        {
            var logger = _logger;

            if (string.IsNullOrEmpty(setKey))
                return new List<T>();

            if (where == null)
                return new List<T>();

            var isLocked = await _lockProvider.IsLockedAsync(setKey);

            //if is locked wait
            if (isLocked)
            {
                var items = new List<T>();
                //warten und die fertige liste zurück geben
                logger.LogDebug("LoadSetItems: Locked - waiting {Key}", setKey);

                await _lockProvider.TryUsingAsync(setKey, async () =>
                {
                    var temp = await LoadList1<T>(setKey, minutes, cancellationToken);
                    items.AddRange(temp);

                }, null, TimeSpan.FromSeconds(_options.CacheLockTimeoutSeconds));


                return items;
            }

            var existsAsync = await CacheClient.ExistsAsync(setKey);

            if (!existsAsync)
            {
                var items = new List<T>();

                logger.LogDebug("LoadSetItems: Key not Found in Cache {Key}", setKey);
                await _lockProvider.TryUsingAsync(setKey, async () =>
                {
                    var temp = await LoadListAndAddSetToCache(setKey, where, minutes, cancellationToken);
                    items.AddRange(temp);
                }, null, TimeSpan.FromSeconds(_options.CacheLockTimeoutSeconds));

                return items;
            }

            return await LoadList1<T>(setKey, minutes, cancellationToken);

        }

        private async Task<List<T>> LoadList1<T>(string setKey, int minutes = int.MinValue, CancellationToken cancellationToken = default) where T : Entity
        {
            var items = new List<T>();

            var idsListValue = await CacheClient.GetSetAsync<string>(setKey);

            if (idsListValue == null)
                return new List<T>();


            if (idsListValue.HasValue)
            {

                var idList = idsListValue.Value;

                if (idList == null)
                    return new List<T>();

                if (IsEmptyList(idList))
                    return items;

                var cacheValues = await CacheClient.GetAllAsync<T>(idList);

                items.AddRange(cacheValues.Values.Where(x => x.HasValue).Select(x => x.Value));

                if (cacheValues.Any(x => !x.Value.HasValue))
                {
                    _logger.LogDebug("Load1: Loading some Missing Cache Values - {Key}", setKey);

                    var expiresIn = GetExpiresIn(minutes);

                    var loadedIds = items.Select(x => x.EntityId).Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    var allMissingIds = idList.Where(id => !loadedIds.Contains(id)).ToArray();

                    foreach (var missingIds in Split(allMissingIds, _options.CacheLoadSplitSize))
                    {
                        var entities = await _entityStore.LoadItemsAsync<T>(x => missingIds.Contains(x.EntityId), cancellationToken);

                        if (entities == null)
                            return items;

                        foreach (var item in entities)
                            await CacheClient.AddAsync(item.EntityId, item, expiresIn);

                        items.AddRange(entities);
                    }
                }

            }

            return items;
        }

        public async Task<T[]> LoadListAndAddSetToCache<T>(string setKey, Expression<Func<T, bool>> where, int minutes = int.MinValue, CancellationToken cancellationToken = default) where T : Entity
        {
            var logger = _logger;

            logger.LogDebug("LoadListAndAddSetToCache {setKey}", setKey);

            var expiresIn = GetExpiresIn(minutes);

            var entities = await _entityStore.LoadItemsAsync(where, cancellationToken);

            if (entities.Length > 0)
            {
                foreach (var item in entities)
                    await CacheClient.AddAsync(item.EntityId, item, expiresIn);

                var entityIds = entities.Select(x => x.EntityId).ToArray();
                await CacheClient.SetAddAsync(setKey, entityIds, expiresIn);
            }
            else
            {
                await CacheClient.SetAddAsync(setKey, EmptyList, expiresIn);
            }

            return entities;
        }

        public async Task SetItemsEnsureAsync(string setKey, string entityId)
        {
            var lockId = $"{setKey}.ItemsAdd";

            await _lockProvider.TryUsingAsync(lockId, async () =>
            {
                if (await CacheClient.ExistsAsync(setKey))
                {
                    await CacheClient.SetAddAsync(setKey, new[] { entityId });
                }
            }, TimeSpan.FromSeconds(2), CancellationToken.None);
        }

        public async Task SetItemsEnsureNotAsync(string setKey, string entityId)
        {
            var lockId = $"{setKey}.ItemsAdd";

            await _lockProvider.TryUsingAsync(lockId, async () =>
            {
                if (await CacheClient.ExistsAsync(setKey))
                {
                    await CacheClient.SetRemoveAsync(setKey, new[] { entityId });
                }
            }, TimeSpan.FromSeconds(2), CancellationToken.None);
        }

        public ICacheClient CacheClient { get; }

        public async Task<T> CacheLoad<T>(Func<Task<T>> loader, string cacheKey, int minutes = int.MinValue, bool setExpirationOnHit = true)
        {
            var logger = _logger;

            logger.LogDebug("CacheLoad for EntityType: {EntityType} with Key {CacheKey}", typeof(T), cacheKey);

            var expiresIn = GetExpiresIn(minutes);

            if (await CacheClient.ExistsAsync(cacheKey))
            {
                logger.LogDebug("Cache Key exists {CacheKey}", cacheKey);

                var l = await CacheClient.GetAsync<T>(cacheKey);
                if (l.HasValue)
                {
                    if (setExpirationOnHit)
                    {
                        await CacheClient.SetExpirationAsync(cacheKey, expiresIn);
                    }
                    logger.LogDebug("Return result from Cache with {CacheKey}", cacheKey);
                    return l.Value;
                }
            }

            logger.LogDebug("No Cache Result {CacheKey}", cacheKey);

            var ret = await loader();


            if (ret != null)
            {
                var isLocked = await _lockProvider.IsLockedAsync(cacheKey);
                if (!isLocked)
                {
                    await _lockProvider.TryUsingAsync(cacheKey, async () =>
                    {
                        await CacheClient.SetAsync(cacheKey, ret, expiresIn);
                    }, null, TimeSpan.FromSeconds(_options.CacheLockTimeoutSeconds));
                }
            }


            return ret;
        }

        public async Task<T[]> CacheLoadItems<T>(Func<Task<T[]>> loader, string cacheKey, int minutes = int.MinValue, bool setExpirationOnHit = true)
        {
            var logger = _logger;

            logger.LogDebug("CacheLoad for EntityType: {EntityType} with Key {CacheKey}", typeof(T), cacheKey);

            var expiresIn = GetExpiresIn(minutes);

            if (await CacheClient.ExistsAsync(cacheKey))
            {
                logger.LogDebug("Cache Key {CacheKey} exists", cacheKey);

                var l = await CacheClient.GetSetAsync<T>(cacheKey);
                if (l.HasValue)
                {
                    logger.LogDebug("Return result from Cache");
                    if (setExpirationOnHit)
                    {
                        await CacheClient.SetExpirationAsync(cacheKey, expiresIn);
                    }
                    return l.Value.ToArray();
                }
            }

            logger.LogDebug("No Cache Result. Loading and Set to Cache");

            var ret = await loader();

            if (ret != null && ret.Length > 0)
            {
                var isLocked = await _lockProvider.IsLockedAsync(cacheKey);

                if (!isLocked)
                {
                    await _lockProvider.TryUsingAsync(cacheKey, async () =>
                    {
                        await CacheClient.SetAddAsync(cacheKey, ret, expiresIn);
                    }, null, TimeSpan.FromSeconds(_options.CacheLockTimeoutSeconds));
                }
            }

            return ret;
        }


        public Task<int> RemoveByPrefixAsync(string prefix)
        {
            return CacheClient.RemoveByPrefixAsync(prefix);
        }

        private TimeSpan GetExpiresIn(int minutes)
        {
            var expiresIn = TimeSpan.FromSeconds(minutes == int.MinValue ? _options.CacheMinutes : minutes);
            return expiresIn;
        }

        private static bool IsEmptyList(ICollection<string> list)
        {
            return list != null && list.Count == 1 && list.Contains(EmptyKey);
        }

        //private async Task<bool> IsPrefixLocked(string cacheKey)
        //{
        //    var splits = cacheKey.Split('.');
        //    if (splits.Length > 0)
        //    {
        //        var prefix = splits[0];
        //        return await _lockProvider.IsLockedAsync($"CACHE-{prefix}");
        //    }

        //    return false;
        //}

        private static IEnumerable<IEnumerable<T>> Split<T>(T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++) yield return array.Skip(i * size).Take(size);
        }
    }
}