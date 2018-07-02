using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Foundatio.Caching;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.ReadModel.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InMemoryEntityStore : EntityStoreBase
    {
        private readonly InMemoryEntityStoreConnectionProvider _provider;
        //private readonly IOptions<InMemoryEntityStoreOptions> _options;


        public InMemoryEntityStore(ILoggerFactory loggerFactory, ICacheClient cacheClient,
            InMemoryEntityStoreConnectionProvider provider
            //, IOptions<InMemoryEntityStoreOptions> options
            ) : base(loggerFactory, cacheClient)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            //_options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public override Task<T[]> LoadItemsAsync<T>()
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("LoadItemsAsync: {EntityType}", typeof(T));
            }

            var items = _provider.GetItems(typeof(T));
            return Task.FromResult(items.Values.Cast<T>().ToArray());
        }

        public override Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("LoadItemsAsync: {EntityType} {Expression}", typeof(T), where);
            }

            var items = _provider.GetItems(typeof(T));

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("LoadItemsAsync: AllItemsCount {EntityType} {AllItemsCount} {Expression}", typeof(T),
                    items.Count, where);
            }

            var witems = items.Values.Cast<T>().AsQueryable().Where(where).ToArray();

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("LoadItemsAsync: FilteredItemsCount {EntityType} {FilteredItemsCount} {Expression}",
                    typeof(T), items.Count, where);
            }

            return Task.FromResult(witems);
        }

        public override Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms)
        {
            if (parms == null)
            {
                throw new ArgumentNullException(nameof(parms));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("LoadPagedResultAsync: {EntityType} {Paramter}", typeof(T), parms);
            }

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

            return Task.FromResult(res);
        }

        protected override Task InternalDeleteItemAsync<T>(string entityId)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("InternalDeleteItemAsync: {EntityType} {EntityId}", typeof(T), entityId);
            }


            var items = _provider.GetItems(typeof(T));

            if (items.ContainsKey(entityId))
            {
                items.Remove(entityId);
            }

            return Task.CompletedTask;
        }

        protected override Task InternalInsertItemAsync<T>(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("InternalInsertItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);
            }

            var items = _provider.GetItems(typeof(T));

            if (items.ContainsKey(item.EntityId))
            {
                items.Remove(item.EntityId);
            }

            items.Add(item.EntityId, item);

            return Task.CompletedTask;
        }

        protected override Task InternalInsertOrUpdateItemAsync<T>(T item)
        {
            return InternalInsertItemAsync(item);
        }

        protected override Task<T> InternalLoadItemAsync<T>(string entityId)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("InternalLoadItemAsync: {EntityType} {EntityId}", typeof(T), entityId);
            }

            var items = _provider.GetItems(typeof(T));

            if (items.ContainsKey(entityId))
            {
                return Task.FromResult((T)items[entityId]);
            }

            return Task.FromResult(default(T));
        }

        protected override async Task InternalUpdateItemAsync<T>(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            await InternalDeleteItemAsync<T>(item.EntityId);
            await InternalInsertItemAsync(item);

        }


    }
}