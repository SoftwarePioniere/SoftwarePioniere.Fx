using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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

            var collection = _provider.GetColLoadItems<T>();
            var filter =
                new ExpressionFilterDefinition<T>(x => x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>());

            var find = await collection.FindAsync(filter, null, cancellationToken);
            var list = await find.ToListAsync(cancellationToken);

            return list.ToArray();

            //var items = await collection.FindAsync(filter, null, cancellationToken);
            //var ret = new List<T>();
            //while (await items.MoveNextAsync(cancellationToken)) ret.AddRange(items.Current);
            //return ret.ToArray();
        }

        public override async Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("LoadItemsAsync: {EntityType} {Expression}", typeof(T), predicate);

            var collection = _provider.GetColLoadItems<T>();
            var filter = new ExpressionFilterDefinition<T>(predicate);

            var find = await collection.FindAsync(filter, null, cancellationToken);
            var list = await find.ToListAsync(cancellationToken);

            return list.ToArray();

            //var items = await collection.FindAsync(filter, null, cancellationToken);

            //var ret = new List<T>();

            //while (await items.MoveNextAsync(cancellationToken)) ret.AddRange(items.Current);

            //return ret.Where(x => x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>()).ToArray();
        }

        protected override async Task InternalBulkInsertItemsAsync<T>(T[] items,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Logger.LogTrace("BulkInsertItemsAsync: {EntityType} {EntityCount}", typeof(T), items.Length);

            try
            {
                var collection = _provider.GetColInsert<T>();

                //var entities = items.Select(item => new MongoEntity<T> { _id = item.EntityId, Entity = item });

                await collection.InsertManyAsync(items, new InsertManyOptions()
                {
                    BypassDocumentValidation = true
                }, cancellationToken);
            }
            catch (MongoException e)
            {
                Logger.LogWarning(e, "InsertManyAsync Failed {ItemsCount} {@Ids}", items.Length, items.Select(x => x.EntityId).ToArray());
            }
        }

        protected override async Task InternalDeleteAllItemsAsync<T>(CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("InternalDeleteAllItemsAsync: {EntityType} ", typeof(T));

            var collectionName = _provider.KeyCache.GetEntityTypeKey<T>();
            try
            {
                await _provider.Database.Value.DropCollectionAsync(collectionName, cancellationToken);
            }
            catch (MongoException e)
            {
                Logger.LogWarning(e, "DropCollectionAsync Failed {CollectionName}", collectionName);
            }
        }

        protected override async Task InternalDeleteItemAsync<T>(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            Logger.LogTrace("InternalDeleteItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            try
            {
                var collection = _provider.GetCol<T>();
                var filter = new ExpressionFilterDefinition<T>(x => x.EntityId == entityId);
                await collection.DeleteOneAsync(filter, cancellationToken);
            }
            catch (MongoException e)
            {
                Logger.LogWarning(e, "DeleteOneAsync Failed {EntityID}", entityId);
            }
        }

        protected override async Task InternalDeleteItemsAsync<T>(Expression<Func<T, bool>> where,
            CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("InternalDeleteItemsAsync: {EntityType} ", typeof(T));

            var filter = new ExpressionFilterDefinition<T>(where);
            try
            {
                var collection = _provider.GetCol<T>();

                await collection.DeleteManyAsync(filter, cancellationToken);
            }
            catch (MongoException e)
            {
                Logger.LogWarning(e, "DeleteManyAsync Failed {Filter}", filter.ToJson());
            }
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

            var collection = _provider.GetColLoadItem<T>();

            var filter = new ExpressionFilterDefinition<T>(x => x.EntityId == entityId);

            var find = await collection.FindAsync(filter, new FindOptions<T>
            {
                Limit = 1
            }, cancellationToken);

            var list = await find.ToListAsync(cancellationToken);

            if (list == null || list.Count == 0)
                return null;

            return list[0];

            //var exi = await collection
            //        .FindAsync(filter, null, cancellationToken)

            //    ;

            //if (await exi.MoveNextAsync(cancellationToken))
            //{
            //    var cu = exi.Current.FirstOrDefault();
            //    if (cu != null)
            //    {
            //        return cu;
            //    }
            //}

            //return null;
        }

        protected override async Task InternalUpdateItemAsync<T>(T item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Logger.LogTrace("InternalUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

            var collection = _provider.GetColInsert<T>();
            var filter = new ExpressionFilterDefinition<T>(x =>
                x.EntityType == _provider.KeyCache.GetEntityTypeKey<T>() && x.EntityId == item.EntityId);

            try
            {
                var res = await collection.ReplaceOneAsync(filter, item, new ReplaceOptions()
                {
                    BypassDocumentValidation = true,
                    IsUpsert = true
                }, cancellationToken);

                Logger.LogDebug("Replace Result: {@Result}", res);
            }
            catch (MongoException e)
            {
                Logger.LogWarning(e, "ReplaceOneAsync Failed: {EntityId} {@Entity}", item.EntityId, item);

                //Logger.LogWarning(e, "Update Failed, Try Insert {EntityId}", item.EntityId);
                //if (res != null && res.IsAcknowledged && res.IsModifiedCountAvailable && res.MatchedCount == 0)
                //{
                //    await collection.InsertOneAsync(item, null, cancellationToken).ConfigureAwait(false);
                //}
            }

        }
    }
}