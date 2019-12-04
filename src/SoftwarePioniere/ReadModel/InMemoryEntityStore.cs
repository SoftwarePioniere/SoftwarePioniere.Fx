using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoftwarePioniere.ReadModel
{

    // ReSharper disable once ClassNeverInstantiated.Global
    public class InMemoryEntityStore : EntityStoreBase<InMemoryEntityStoreOptions>
    {
        private readonly InMemoryEntityStoreConnectionProvider _provider;

        public InMemoryEntityStore(IOptions<InMemoryEntityStoreOptions> options,
            InMemoryEntityStoreConnectionProvider provider,
            ILoggerFactory loggerFactory, ICacheClient cacheClient
            ) : base(options, loggerFactory, cacheClient)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public override Task<T[]> LoadItemsAsync<T>(CancellationToken cancellationToken = default)
        {

            Logger.LogTrace("LoadItemsAsync: {EntityType}", typeof(T));

            var items = _provider.GetItems(typeof(T));
            return Task.FromResult(items.Values.Cast<T>().ToArray());
        }

        public override Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
        {

            Logger.LogTrace("LoadItemsAsync: {EntityType} {Expression}", typeof(T), where);


            var items = _provider.GetItems(typeof(T));


            Logger.LogTrace("LoadItemsAsync: AllItemsCount {EntityType} {AllItemsCount} {Expression}", typeof(T),
                items.Count, where);


            var witems = items.Values.Cast<T>().AsQueryable().Where(where).ToArray();


            Logger.LogTrace("LoadItemsAsync: FilteredItemsCount {EntityType} {FilteredItemsCount} {Expression}", typeof(T), items.Count, where);
            cancellationToken.ThrowIfCancellationRequested();


            return Task.FromResult(witems);
        }
        
        protected override Task InternalDeleteItemAsync<T>(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }


            Logger.LogTrace("InternalDeleteItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            cancellationToken.ThrowIfCancellationRequested();

            var items = _provider.GetItems(typeof(T));
            // ReSharper disable once UnusedVariable
            items.TryRemove(entityId, out var value);

            return Task.CompletedTask;
        }

        protected override Task InternalDeleteItemsAsync<T>(Expression<Func<T, bool>> @where, CancellationToken cancellationToken = default)
        {

            Logger.LogTrace("InternalDeleteItemsAsync: {EntityType}", typeof(T));


            var items = _provider.GetItems(typeof(T));

            var values = items.Values.Cast<T>();

            var w = @where.Compile();
            var filteredItems = values.Where(w);

            foreach (var id in filteredItems.Select(x => x.EntityId).ToArray())
            {
                // ReSharper disable once UnusedVariable
                items.TryRemove(id, out var value);
            }

            return Task.CompletedTask;
        }

        protected override Task InternalDeleteAllItemsAsync<T>(CancellationToken cancellationToken = default)
        {
            return InternalDeleteItemsAsync<T>(x => true, cancellationToken);
        }

        protected override Task InternalInsertItemAsync<T>(T item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }


            Logger.LogTrace("InternalInsertItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);


            cancellationToken.ThrowIfCancellationRequested();

            var items = _provider.GetItems(typeof(T));

            items.AddOrUpdate(item.EntityId,
                item, (s, o) => item);

            return Task.CompletedTask;
        }

        protected override Task InternalBulkInsertItemsAsync<T>(T[] items, CancellationToken cancellationToken = default)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }


            Logger.LogTrace("BulkInsertItemsAsync: {EntityType} {EntityCount}", typeof(T), items.Length);


            cancellationToken.ThrowIfCancellationRequested();

            var localItems = _provider.GetItems(typeof(T));

            foreach (var item in items)
            {

                localItems.AddOrUpdate(item.EntityId,
                    item, (s, o) => item);
            }

            return Task.CompletedTask;
        }

        protected override Task InternalInsertOrUpdateItemAsync<T>(T item, CancellationToken cancellationToken = default)
        {
            return InternalInsertItemAsync(item, cancellationToken);
        }

        protected override Task<T> InternalLoadItemAsync<T>(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }


            Logger.LogTrace("InternalLoadItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            cancellationToken.ThrowIfCancellationRequested();

            var items = _provider.GetItems(typeof(T));

            if (items.ContainsKey(entityId))
            {
                return Task.FromResult((T)items[entityId]);
            }

            return Task.FromResult(default(T));
        }

        protected override async Task InternalUpdateItemAsync<T>(T item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await InternalDeleteItemAsync<T>(item.EntityId, cancellationToken);
            await InternalInsertItemAsync(item, cancellationToken);

        }


    }
}