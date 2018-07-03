using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SoftwarePioniere.ReadModel.Services
{
    public abstract class EntityStoreBase : IEntityStore
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly ICacheClient CacheClient;
        protected readonly ILogger Logger;
        protected readonly TypeKeyCache TypeKeyCache = new TypeKeyCache();

        protected EntityStoreBase(ILoggerFactory loggerFactory, ICacheClient cacheClient)
        {
            CacheClient = cacheClient ?? throw new ArgumentNullException(nameof(cacheClient));
            Logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger(GetType());
        }

        public virtual async Task DeleteItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("DeleteItemAsync: {EntityType} {EntityId}", typeof(T), entityId);
            }

            await ClearCache<T>();
            await InternalDeleteItemAsync<T>(entityId, token);
        }

        public async Task InsertItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("InsertItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);
            }

            await ClearCache<T>();
            await InternalInsertItemAsync(item, token);
        }


        public async Task InsertOrUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("InsertOrUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);
            }

            await ClearCache<T>();
            await InternalInsertOrUpdateItemAsync(item, token);
        }

        public async Task<T> LoadItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("LoadItemAsync: {EntityType} {EntityId}", typeof(T), entityId);
            }

            return await CacheLoad(() => InternalLoadItemAsync<T>(entityId, token), CacheKeys.Create<T>(nameof(T), entityId));
        }

        public abstract Task<T[]> LoadItemsAsync<T>(CancellationToken token = default(CancellationToken)) where T : Entity;

        public abstract Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, CancellationToken token = default(CancellationToken)) where T : Entity;

        public async Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, string cacheKey, CancellationToken token = default(CancellationToken)) where T : Entity
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }


            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("LoadItemsAsync: {EntityType} {Expression} {CacheKey}", typeof(T), where, cacheKey);
            }

            return await CacheLoad(() => LoadItemsAsync(where, token), cacheKey);
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

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("LoadItemsAsync: {EntityType} {PagedLoadingParameters}  {CacheKey}", typeof(T), parms,
                    cacheKey);
            }

            return await CacheLoad(() => LoadPagedResultAsync(parms, token), cacheKey);
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

            await ClearCache<T>();
            await InternalUpdateItemAsync(item, token);
        }

        protected abstract Task InternalDeleteItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity;

        protected abstract Task InternalInsertItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;

        protected abstract Task InternalInsertOrUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;

        protected abstract Task<T> InternalLoadItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity;

        protected abstract Task InternalUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;

        private Task<T> CacheLoad<T>(Func<Task<T>> loader, string cacheKey)
        {
            return CacheClient.CacheLoad(loader, cacheKey, 30, Logger);
        }

        private Task ClearCache<T>() where T : Entity
        {
            var t = Activator.CreateInstance<T>();
            var cachePrefix = t.EntityType;

            if (!string.IsNullOrEmpty(cachePrefix))
            {
                if (Logger.IsEnabled(LogLevel.Debug))
                {
                    Logger.LogDebug("Clearing Cache for EntityType: {EntityType}", typeof(T));
                }

                return CacheClient.RemoveByPrefixAsync(cachePrefix);
            }

            return Task.CompletedTask;
        }
    }
}