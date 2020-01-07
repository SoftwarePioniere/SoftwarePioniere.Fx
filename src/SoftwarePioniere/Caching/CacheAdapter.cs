using System;
using System.Collections.Generic;
using System.Diagnostics;
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


        protected bool LogError(Exception ex)
        {
            _logger.LogError(ex, ex.GetBaseException().Message);
            return true;
        }

        public async Task<List<T>> LoadSetItems<T>(string setKey, Expression<Func<T, bool>> where, int minutes = int.MinValue, CancellationToken cancellationToken = default) where T : Entity
        {

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                {"SetKey", setKey},
                {"EntityType", typeof(T).Name}
            }))
            {

                var sw = Stopwatch.StartNew();
                _logger.LogDebug("LoadSetItems started");
                if (string.IsNullOrEmpty(setKey))
                    return new List<T>();

                if (where == null)
                    return new List<T>();

                if (!_options.DisableLocking)
                {
                    var isLocked = await _lockProvider.IsLockedAsync(setKey);

                    //if is locked wait
                    if (isLocked)
                    {
                        var items = new List<T>();
                        //warten und die fertige liste zurück geben
                        _logger.LogDebug("LoadSetItems: Locked - waiting {Key}", setKey);

                        await _lockProvider.TryUsingAsync(setKey, async () =>
                        {
                            var temp = await LoadList1<T>(setKey, minutes, cancellationToken);
                            items.AddRange(temp);

                        }, null, TimeSpan.FromSeconds(_options.CacheLockTimeoutSeconds));


                        return items;
                    }
                }

                var existsAsync = await CacheClient.ExistsAsync(setKey);

                if (!existsAsync)
                {
                    var items = new List<T>();

                    _logger.LogDebug("LoadSetItems: Key not Found in Cache {Key}", setKey);

                    if (_options.DisableLocking)
                    {
                        var temp = await LoadListAndAddSetToCache(setKey, where, minutes, cancellationToken);
                        items.AddRange(temp);
                    }
                    else
                    {

                        await _lockProvider.TryUsingAsync(setKey, async () =>
                        {
                            var temp = await LoadListAndAddSetToCache(setKey, where, minutes, cancellationToken);
                            items.AddRange(temp);
                        }, null, TimeSpan.FromSeconds(_options.CacheLockTimeoutSeconds));

                    }
                    return items;
                }


                var result = await LoadList1<T>(setKey, minutes, cancellationToken);

                sw.Stop();
                _logger.LogDebug("LoadSetItems finished in {Elapsed} ms", sw.ElapsedMilliseconds);

                return result;
            }
        }

        private async Task<List<T>> LoadList1<T>(string setKey, int minutes = int.MinValue, CancellationToken cancellationToken = default) where T : Entity
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("LoadList1 started");

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
                        {
                            await CacheClient.AddAsync(item.EntityId, item, expiresIn);
                            items.Add(item);
                        }

                    }
                }

            }


            sw.Stop();
            _logger.LogDebug("LoadList1 finished in {Elapsed} ms", sw.ElapsedMilliseconds);
            return items;
        }

        public async Task<T[]> LoadListAndAddSetToCache<T>(string setKey, Expression<Func<T, bool>> where, int minutes = int.MinValue, CancellationToken cancellationToken = default) where T : Entity
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                {"SetKey", setKey},
                {"EntityType", typeof(T).Name}
            }))
            {

                var sw = Stopwatch.StartNew();
                _logger.LogDebug("LoadListAndAddSetToCache started");

                var expiresIn = GetExpiresIn(minutes);

                try
                {
                    var tmp = await _entityStore.LoadItemsAsync(where, cancellationToken);
                    var entities = tmp.ToArray();

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

                    sw.Stop();
                    _logger.LogDebug("LoadListAndAddSetToCache finished in {Elapsed} ms", sw.ElapsedMilliseconds);

                    return entities;
                }
                catch (Exception e) when (LogError(e))
                {
                    throw;
                }
            }
        }

        public async Task SetItemsEnsureAsync(string setKey, string entityId)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                {"SetKey", setKey},
                {"EntityId", entityId}
            }))
            {
                var sw = Stopwatch.StartNew();
                _logger.LogDebug("SetItemsEnsureAsync started");

                if (await CacheClient.ExistsAsync(setKey))
                {

                    if (_options.DisableLocking2)
                    {
                        //if (await CacheClient.ExistsAsync(setKey))
                        //{
                        await CacheClient.SetAddAsync(setKey, new[] { entityId });
                        //}
                    }
                    else
                    {

                        var lockId = $"{setKey}.ItemsAdd";

                        await _lockProvider.TryUsingAsync(lockId, async () => { await CacheClient.SetAddAsync(setKey, new[] { entityId }); }, TimeSpan.FromSeconds(2), CancellationToken.None);

                    }
                }

                sw.Stop();
                _logger.LogDebug("SetItemsEnsureAsync finished in {Elapsed} ms", sw.ElapsedMilliseconds);
            }
        }

        public async Task SetItemsEnsureNotAsync(string setKey, string entityId)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                {"SetKey", setKey},
                {"EntityId", entityId}
            }))
            {

                var sw = Stopwatch.StartNew();
                _logger.LogDebug("SetItemsEnsureNotAsync started");

                if (await CacheClient.ExistsAsync(setKey))
                {

                    if (_options.DisableLocking2)
                    {
                        //if (await CacheClient.ExistsAsync(setKey))
                        //{
                        await CacheClient.SetRemoveAsync(setKey, new[] { entityId });
                        //}
                    }
                    else
                    {
                        var lockId = $"{setKey}.ItemsAdd";

                        await _lockProvider.TryUsingAsync(lockId, async () => { await CacheClient.SetRemoveAsync(setKey, new[] { entityId }); }, TimeSpan.FromSeconds(2), CancellationToken.None);
                    }
                }

                sw.Stop();
                _logger.LogDebug("SetItemsEnsureNotAsync finished in {Elapsed} ms", sw.ElapsedMilliseconds);
            }
        }

        public ICacheClient CacheClient { get; }

        public async Task<T> CacheLoad<T>(Func<Task<T>> loader, string cacheKey, int minutes = int.MinValue, bool setExpirationOnHit = true)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                {"CacheKey", cacheKey},
                {"EntityType", typeof(T).Name}
            }))
            {

                var sw = Stopwatch.StartNew();
                _logger.LogDebug("CacheLoad started");


                var expiresIn = GetExpiresIn(minutes);

                if (await CacheClient.ExistsAsync(cacheKey))
                {
                    _logger.LogDebug("Cache Key exists", cacheKey);

                    var l = await CacheClient.GetAsync<T>(cacheKey);
                    if (l.HasValue)
                    {
                        if (setExpirationOnHit)
                        {
                            await CacheClient.SetExpirationAsync(cacheKey, expiresIn);
                        }

                        _logger.LogDebug("Return result from Cache with {CacheKey}", cacheKey);

                        sw.Stop();
                        _logger.LogDebug("CacheLoad finished in {Elapsed} ms", sw.ElapsedMilliseconds);

                        return l.Value;
                    }
                }

                _logger.LogDebug("No Cache Result");

                try
                {
                    var ret = await loader();

                    if (ret != null)
                    {
                        if (_options.DisableLocking3)
                        {
                            await CacheClient.SetAsync(cacheKey, ret, expiresIn);
                        }
                        else
                        {

                            var isLocked = await _lockProvider.IsLockedAsync(cacheKey);
                            if (!isLocked)
                            {
                                await _lockProvider.TryUsingAsync(cacheKey, async () => { await CacheClient.SetAsync(cacheKey, ret, expiresIn); }, null, TimeSpan.FromSeconds(_options.CacheLockTimeoutSeconds));
                            }
                        }
                    }


                    sw.Stop();
                    _logger.LogDebug("CacheLoad finished in {Elapsed} ms", sw.ElapsedMilliseconds);

                    return ret;

                }
                catch (Exception e) when (LogError(e))
                {
                    throw;
                }

            }
        }

        public async Task<T[]> CacheLoadItems<T>(Func<Task<IEnumerable<T>>> loader, string cacheKey, int minutes = int.MinValue, bool setExpirationOnHit = true)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                {"CacheKey", cacheKey},
                {"EntityType", typeof(T).Name}
            }))
            {

                var sw = Stopwatch.StartNew();
                _logger.LogDebug("CacheLoadItems started");

                var expiresIn = GetExpiresIn(minutes);

                if (await CacheClient.ExistsAsync(cacheKey))
                {
                    _logger.LogDebug("Cache Key exists");

                    var l = await CacheClient.GetSetAsync<T>(cacheKey);
                    if (l.HasValue)
                    {
                        _logger.LogDebug("Return result from Cache");
                        if (setExpirationOnHit)
                        {
                            await CacheClient.SetExpirationAsync(cacheKey, expiresIn);
                        }

                        sw.Stop();
                        _logger.LogDebug("CacheLoadItems finished in {Elapsed} ms", sw.ElapsedMilliseconds);

                        return l.Value.ToArray();
                    }
                }

                _logger.LogDebug("No Cache Result. Loading and Set to Cache");

                try
                {
                    var ret = await loader();

                    var enumerable = ret as T[] ?? ret.ToArray();

                    if (enumerable.Any())
                    {
                        if (_options.DisableLocking3)
                        {
                            await CacheClient.SetAddAsync(cacheKey, enumerable, expiresIn);
                        }
                        else
                        {

                            var isLocked = await _lockProvider.IsLockedAsync(cacheKey);

                            if (!isLocked)
                            {
                                await _lockProvider.TryUsingAsync(cacheKey, async () => { await CacheClient.SetAddAsync(cacheKey, enumerable, expiresIn); }, null, TimeSpan.FromSeconds(_options.CacheLockTimeoutSeconds));
                            }
                        }
                    }


                    sw.Stop();
                    _logger.LogDebug("CacheLoadItems finished in {Elapsed} ms", sw.ElapsedMilliseconds);
                    return enumerable;
                }
                catch (Exception e) when (LogError(e))
                {
                    throw;
                }

            }
        }


        public async Task<int> RemoveByPrefixAsync(string prefix)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                {"Prefix", prefix},
            }))
            {
                var sw = Stopwatch.StartNew();
                _logger.LogDebug("RemoveByPrefixAsync started");

                var result = await CacheClient.RemoveByPrefixAsync(prefix);

                sw.Stop();
                _logger.LogDebug("RemoveByPrefixAsync finished in {Elapsed} ms", sw.ElapsedMilliseconds);
                return result;
            }
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