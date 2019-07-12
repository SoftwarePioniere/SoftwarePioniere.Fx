using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SoftwarePioniere.ReadModel
{

    public abstract class EntityStoreBase<TOptions> : IEntityStore where TOptions : EntityStoreOptionsBase
    {

        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly TOptions Options;
        protected readonly ICacheClient CacheClient;
        protected readonly ILogger Logger;
        protected readonly TypeKeyCache TypeKeyCache = new TypeKeyCache();

        protected EntityStoreBase(TOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            var loggerFactory = options.LoggerFactory ?? NullLoggerFactory.Instance;
            Logger = loggerFactory.CreateLogger(GetType());
            if (!Options.CachingDisabled)
            {
                CacheClient = Options.CacheClient ?? throw new ArgumentNullException(nameof(Options.CacheClient));
            }
        }

        public virtual async Task DeleteItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            Logger.LogDebug("DeleteItemAsync: {EntityType} {EntityId}", typeof(T), entityId);


            if (!Options.CachingDisabled)
            {
                await ClearCache<T>();
            }

            await InternalDeleteItemAsync<T>(entityId, token);
        }

        public async Task InsertItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.LogDebug("InsertItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

            if (!Options.CachingDisabled)
            {
                await ClearCache<T>();
            }
            await InternalInsertItemAsync(item, token);
        }

        public async Task BulkInsertItemsAsync<T>(IEnumerable<T> items, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var enumerable = items as T[] ?? items.ToArray();
            Logger.LogDebug("BulkInsertItemsAsync: {EntityType} {EntityCount}", typeof(T), enumerable.Count());


            if (!Options.CachingDisabled)
            {
                await ClearCache<T>();
            }

            await InternalBulkInsertItemsAsync(enumerable, token);
        }


        public async Task InsertOrUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }


            Logger.LogDebug("InsertOrUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);


            if (!Options.CachingDisabled)
            {
                await ClearCache<T>();
            }
            await InternalInsertOrUpdateItemAsync(item, token);
        }

        public async Task<T> LoadItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            Logger.LogDebug("LoadItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            if (Options.CachingDisabled)
            {
                return await InternalLoadItemAsync<T>(entityId, token);
            }

            return await CacheLoadItem(() => InternalLoadItemAsync<T>(entityId, token), CacheKeys.Create<T>(nameof(T), entityId));
        }

        public abstract Task<T[]> LoadItemsAsync<T>(CancellationToken token = default(CancellationToken)) where T : Entity;

        public abstract Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, CancellationToken token = default(CancellationToken)) where T : Entity;

        public async Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, string cacheKey, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            Logger.LogDebug("LoadItemsAsync: {EntityType} {Expression} {CacheKey}", typeof(T), where, cacheKey);

            if (Options.CachingDisabled)
            {
                return await LoadItemsAsync(where, token);
            }

            return await CacheLoadItems(() => LoadItemsAsync(where, token), cacheKey);
        }

        public abstract Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms, CancellationToken token = default(CancellationToken)) where T : Entity;

        public async Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms, string cacheKey, CancellationToken token = default(CancellationToken))
            where T : Entity
        {
            if (parms == null)
            {
                throw new ArgumentNullException(nameof(parms));
            }

            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            Logger.LogDebug("LoadItemsAsync: {EntityType} {PagedLoadingParameters}  {CacheKey}", typeof(T), parms, cacheKey);

            if (Options.CachingDisabled)
            {
                return await LoadPagedResultAsync(parms, token);
            }

            return await CacheLoadPagedResult(() => LoadPagedResultAsync(parms, token), cacheKey);
        }

        public async Task UpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("UpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);
            }

            if (!Options.CachingDisabled)
            {
                await ClearCache<T>();
            }

            await InternalUpdateItemAsync(item, token);
        }

        protected abstract Task InternalDeleteItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity;

        protected abstract Task InternalInsertItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;

        protected abstract Task InternalBulkInsertItemsAsync<T>(T[] items, CancellationToken token = default(CancellationToken)) where T : Entity;

        protected abstract Task InternalInsertOrUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;

        protected abstract Task<T> InternalLoadItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity;

        protected abstract Task InternalUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;

        private Task<T> CacheLoadItem<T>(Func<Task<T>> loader, string cacheKey)
        {
            return CacheClient.CacheLoadItem(loader, cacheKey, 30, Logger);
        }

        private Task<T[]> CacheLoadItems<T>(Func<Task<T[]>> loader, string cacheKey)
        {
            return CacheClient.CacheLoadItems(loader, cacheKey, 30, Logger);
        }


        private Task<PagedResults<T>> CacheLoadPagedResult<T>(Func<Task<PagedResults<T>>> loader, string cacheKey)
        {
            return CacheClient.CacheLoadPagedItems(loader, cacheKey, 30, Logger);
        }

        private Task ClearCache<T>() where T : Entity
        {
            var t = Activator.CreateInstance<T>();
            var cachePrefix = t.EntityType;

            if (!string.IsNullOrEmpty(cachePrefix))
            {
                if (Logger.IsEnabled(LogLevel.Debug))
                {
                    Logger.LogTrace("Clearing Cache for EntityType: {EntityType}", typeof(T));
                }

                return CacheClient.RemoveByPrefixAsync(cachePrefix);
            }

            return Task.CompletedTask;
        }
    }
}