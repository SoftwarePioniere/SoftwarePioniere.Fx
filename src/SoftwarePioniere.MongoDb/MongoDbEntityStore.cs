using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.MongoDb
{
    public class MongoDbEntityStore : EntityStoreBase<MongoDbOptions>
    {
        private readonly MongoDbConnectionProvider _provider;

        public MongoDbEntityStore(IOptions<MongoDbOptions> options, ILoggerFactory loggerFactory,
            ICacheClient cacheClient, MongoDbConnectionProvider provider) : base(options, loggerFactory, cacheClient)
        {
            _provider = provider;
        }

        public override async Task<T[]> LoadItemsAsync<T>(CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("LoadItemsAsync: {EntityType}", typeof(T));

            var collection = _provider.GetCol<T>();
            var filter =
                new ExpressionFilterDefinition<T>(x => x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>());

            var items = await collection.FindAsync(filter, null, cancellationToken);

            var ret = new List<T>();

            while (await items.MoveNextAsync(cancellationToken)) ret.AddRange(items.Current);

            return ret.ToArray();
        }

        public override async Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("LoadItemsAsync: {EntityType} {Expression}", typeof(T), predicate);

            var collection = _provider.GetCol<T>();
            var filter = new ExpressionFilterDefinition<T>(predicate);

            var items = await collection.FindAsync(filter, null, cancellationToken);

            var ret = new List<T>();

            while (await items.MoveNextAsync(cancellationToken)) ret.AddRange(items.Current);

            return ret.Where(x => x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>()).ToArray();
        }

        public override async Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms,
            CancellationToken cancellationToken = default)
        {
            if (parms == null)
            {
                throw new ArgumentNullException(nameof(parms));
            }

            Logger.LogTrace("LoadPagedResultAsync: {EntityType} {Paramter}", typeof(T), parms);

            IQueryable<T> items;

            if (parms.Where != null)
            {
                items = (await LoadItemsAsync(parms.Where, cancellationToken)).AsQueryable();
            }
            else
            {
                items = (await LoadItemsAsync<T>(cancellationToken)).AsQueryable();
            }

            if (parms.OrderByDescending != null)
            {
                items = items.OrderByDescending(parms.OrderByDescending);
            }

            if (parms.OrderBy != null)
            {
                items = items.OrderBy(parms.OrderBy);
            }

            return items.GetPagedResults(parms.PageSize, parms.Page);
        }

        protected override async Task InternalBulkInsertItemsAsync<T>(T[] items,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Logger.LogTrace("BulkInsertItemsAsync: {EntityType} {EntityCount}", typeof(T), items.Length);

            var collection = _provider.GetCol<T>();

            //var entities = items.Select(item => new MongoEntity<T> { _id = item.EntityId, Entity = item });

            await collection.InsertManyAsync(items, null, cancellationToken);
        }

        protected override async Task InternalDeleteAllItemsAsync<T>(CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("InternalDeleteAllItemsAsync: {EntityType} ", typeof(T));
            await _provider.Database.Value.DropCollectionAsync(_provider.KeyCache.GetEntityTypeKey<T>(), cancellationToken);
        }

        protected override async Task InternalDeleteItemAsync<T>(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            Logger.LogTrace("InternalDeleteItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            var collection = _provider.GetCol<T>();
            var filter = new ExpressionFilterDefinition<T>(x => x.EntityId == entityId);
            await collection.DeleteOneAsync(filter, cancellationToken);
        }

        protected override async Task InternalDeleteItemsAsync<T>(Expression<Func<T, bool>> where,
            CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("InternalDeleteItemsAsync: {EntityType} ", typeof(T));

            var collection = _provider.GetCol<T>();
            var filter = new ExpressionFilterDefinition<T>(where);
            await collection.DeleteManyAsync(filter, cancellationToken);
        }

        protected override Task InternalInsertItemAsync<T>(T item, CancellationToken cancellationToken = default)
        {
            return InternalUpdateItemAsync(item, cancellationToken);
        }

        protected override Task InternalInsertOrUpdateItemAsync<T>(T item, CancellationToken cancellationToken = default)
        {
            return InternalUpdateItemAsync(item, cancellationToken);
        }

        //protected override async Task InternalInsertItemAsync<T>(T item, CancellationToken cancellationToken = default)
        //{
        //    if (item == null)
        //    {
        //        throw new ArgumentNullException(nameof(item));
        //    }

        //    Logger.LogTrace("InternalInsertItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);


        //    var collection = _provider.GetCol<T>();

        //    try
        //    {
        //        await collection.InsertOneAsync(item, new InsertOneOptions()
        //        {
        //            BypassDocumentValidation = false
        //        }, token).ConfigureAwait(false);
        //    }
        //    catch (MongoWriteException e) when (e.WriteError.Code == 11000)
        //    {
        //        Logger.LogWarning("Insert Failed, Try Update {EntityId} // {ExceptionMessage}",
        //            item.EntityId,
        //            e.Message);

        //        if (_provider.Options.ThrowDeveloperError)
        //        {
        //            throw;
        //        }

        //        var filter = new ExpressionFilterDefinition<T>(x =>
        //            x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>() && x.EntityId == item.EntityId);
        //        await collection.ReplaceOneAsync(filter, item, null, token);
        //    }
        //}

        //protected override async Task InternalInsertOrUpdateItemAsync<T>(T item, CancellationToken cancellationToken = default)
        //{
        //    if (item == null)
        //    {
        //        throw new ArgumentNullException(nameof(item));
        //    }

        //    Logger.LogTrace("InternalInsertOrUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

        //    var collection = _provider.GetCol<T>();

        //    var filter = new ExpressionFilterDefinition<T>(x =>
        //        x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>() && x.EntityId == item.EntityId);

        //    var exi = await collection.FindAsync(filter, null, token);

        //    if (await exi.MoveNextAsync(token))
        //    {
        //        await UpdateItemAsync(item, token).ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        await InsertItemAsync(item, token).ConfigureAwait(false);
        //    }
        //}

        protected override async Task<T> InternalLoadItemAsync<T>(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            Logger.LogTrace("InternalLoadItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            var collection = _provider.GetCol<T>();

            var filter = new ExpressionFilterDefinition<T>(x =>
                x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>() && x.EntityId == entityId);

            var exi = await collection.FindAsync(filter, null, cancellationToken);

            if (await exi.MoveNextAsync(cancellationToken))
            {
                var cu = exi.Current.FirstOrDefault();
                if (cu != null)
                {
                    return cu;
                }
            }

            return null;
        }

        protected override async Task InternalUpdateItemAsync<T>(T item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.LogTrace("InternalUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

            var collection = _provider.GetCol<T>();
            var filter = new ExpressionFilterDefinition<T>(x =>
                x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>() && x.EntityId == item.EntityId);

            //  var res = 
            await collection.ReplaceOneAsync(filter, item, new UpdateOptions
            {
                IsUpsert = true
            }, cancellationToken);


            //if (res.MatchedCount == 0)
            //{
            //    Logger.LogWarning("Update Failed, Try Insert {EntityId}", item.EntityId);
            //    await collection.InsertOneAsync(item, null, token).ConfigureAwait(false);
            //}
        }
    }
}