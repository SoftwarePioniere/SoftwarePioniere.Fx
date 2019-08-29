using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.ReadModel
{

    // ReSharper disable once ClassNeverInstantiated.Global
    public class InMemoryEntityStore : EntityStoreBase<InMemoryEntityStoreOptions>
    {
        private readonly InMemoryEntityStoreConnectionProvider _provider;
        //private readonly IOptions<InMemoryEntityStoreOptions> _options;


        public InMemoryEntityStore(InMemoryEntityStoreOptions options,
            InMemoryEntityStoreConnectionProvider provider
            //, IOptions<InMemoryEntityStoreOptions> options
            ) : base(options)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            //_options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public override Task<T[]> LoadItemsAsync<T>(CancellationToken token = default(CancellationToken))
        {

            Logger.LogTrace("LoadItemsAsync: {EntityType}", typeof(T));

            var items = _provider.GetItems(typeof(T));
            return Task.FromResult(items.Values.Cast<T>().ToArray());
        }

        public override Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, CancellationToken token = default(CancellationToken))
        {

            Logger.LogTrace("LoadItemsAsync: {EntityType} {Expression}", typeof(T), where);


            var items = _provider.GetItems(typeof(T));


            Logger.LogTrace("LoadItemsAsync: AllItemsCount {EntityType} {AllItemsCount} {Expression}", typeof(T),
                items.Count, where);


            var witems = items.Values.Cast<T>().AsQueryable().Where(where).ToArray();


            Logger.LogTrace("LoadItemsAsync: FilteredItemsCount {EntityType} {FilteredItemsCount} {Expression}", typeof(T), items.Count, where);
            token.ThrowIfCancellationRequested();


            return Task.FromResult(witems);
        }

        public override Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms, CancellationToken token = default(CancellationToken))
        {
            if (parms == null)
            {
                throw new ArgumentNullException(nameof(parms));
            }


            Logger.LogTrace("LoadPagedResultAsync: {EntityType} {Paramter}", typeof(T), parms);

            var val = _provider.GetItems(typeof(T));

            if (val == null)
            {
                return null;
            }

            var items = val.Values.Cast<T>().AsQueryable();

            if (parms.Where != null)
            {
                items = items.Where(parms.Where);
            }

            if (parms.OrderByDescending != null)
            {
                items = items.OrderByDescending(parms.OrderByDescending);

                //if (parms.OrderThenBy != null)
                //{
                //    items = items.OrderByDescending(parms.OrderByDescending).ThenBy(parms.OrderThenBy);
                //}
                //else
                //{

                //}

            }

            if (parms.OrderBy != null)
            {
                items = items.OrderBy(parms.OrderBy);

                //if (parms.OrderThenBy != null)
                //{
                //    items = items.OrderBy(parms.OrderBy).ThenBy(parms.OrderThenBy);
                //}
                //else
                //{

                //}

            }

            var res = items.GetPagedResults(parms.PageSize, parms.Page);

            token.ThrowIfCancellationRequested();

            return Task.FromResult(res);
        }

        protected override Task InternalDeleteItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }


            Logger.LogTrace("InternalDeleteItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            token.ThrowIfCancellationRequested();

            var items = _provider.GetItems(typeof(T));

            if (items.ContainsKey(entityId))
            {
                items.Remove(entityId);
            }

            return Task.CompletedTask;
        }

        protected override Task InternalDeleteItemsAsync<T>(Expression<Func<T, bool>> @where, CancellationToken token = default(CancellationToken))
        {

            Logger.LogTrace("InternalDeleteItemsAsync: {EntityType}", typeof(T));


            var items = _provider.GetItems(typeof(T));

            var values = items.Values.Cast<T>();

            var w = @where.Compile();
            var filteredItems = values.Where(w);

            foreach (var id in filteredItems.Select(x => x.EntityId).ToArray())
            {
                if (items.ContainsKey(id))
                    items.Remove(id);
            }

            return Task.CompletedTask;
        }

        protected override Task InternalDeleteAllItemsAsync<T>(CancellationToken token = default(CancellationToken))
        {
            return InternalDeleteItemsAsync<T>(x => true, token);
        }

        protected override Task InternalInsertItemAsync<T>(T item, CancellationToken token = default(CancellationToken))
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }


            Logger.LogTrace("InternalInsertItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);


            token.ThrowIfCancellationRequested();

            var items = _provider.GetItems(typeof(T));

            if (items.ContainsKey(item.EntityId))
            {
                items.Remove(item.EntityId);
            }

            items.Add(item.EntityId, item);

            return Task.CompletedTask;
        }

        protected override Task InternalBulkInsertItemsAsync<T>(T[] items, CancellationToken token = default(CancellationToken))
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }


            Logger.LogTrace("BulkInsertItemsAsync: {EntityType} {EntityCount}", typeof(T), items.Length);


            token.ThrowIfCancellationRequested();

            var localItems = _provider.GetItems(typeof(T));

            foreach (var item in items)
            {

                if (localItems.ContainsKey(item.EntityId))
                {
                    localItems.Remove(item.EntityId);
                }

                localItems.Add(item.EntityId, item);
            }

            return Task.CompletedTask;
        }

        protected override Task InternalInsertOrUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken))
        {
            return InternalInsertItemAsync(item, token);
        }

        protected override Task<T> InternalLoadItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }


            Logger.LogTrace("InternalLoadItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            token.ThrowIfCancellationRequested();

            var items = _provider.GetItems(typeof(T));

            if (items.ContainsKey(entityId))
            {
                return Task.FromResult((T)items[entityId]);
            }

            return Task.FromResult(default(T));
        }

        protected override async Task InternalUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken))
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            token.ThrowIfCancellationRequested();

            await InternalDeleteItemAsync<T>(item.EntityId, token);
            await InternalInsertItemAsync(item, token);

        }


    }
}