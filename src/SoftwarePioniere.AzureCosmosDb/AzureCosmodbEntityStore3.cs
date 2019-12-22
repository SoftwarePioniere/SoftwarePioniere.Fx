using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Caching;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.AzureCosmosDb
{
    public class AzureCosmodbEntityStore3 : EntityStoreBase<AzureCosmosDbOptions>
    {
        private readonly AzureComsosDbConnectionProvider3 _provider;

        public AzureCosmodbEntityStore3(IOptions<AzureCosmosDbOptions> options, ILoggerFactory loggerFactory,
            ICacheClient cacheClient, AzureComsosDbConnectionProvider3 provider) : base(options,
            loggerFactory,
            cacheClient)
        {
            _provider = provider;
        }


        //public AzureCosmodbEntityStore3(AzureCosmosDbOptions options,
        //    AzureComsosDbConnectionProvider3 provider) : base(options)
        //{
        //    _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        //}

        public override async Task<IEnumerable<T>> LoadItemsAsync<T>(CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("LoadItemsAsync: {EntityType}", typeof(T));

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var iter = _provider.Container
                        .GetItemLinqQueryable<T>(true)
                        .Where(x => x.EntityType == TypeKeyCache.GetEntityTypeKey<T>())
                        .ToFeedIterator()
                    ;

                var results = new List<T>();

                while (iter.HasMoreResults)
                    foreach (var entity in await iter.ReadNextAsync(cancellationToken))
                        results.Add(entity);

                return results;
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning("LoadItemsAsync: Items not found  {ErrorCode} {StatusCode} {Message}",
                    nfex.StatusCode,
                    nfex.SubStatusCode,
                    nfex.Message);
                return new T[0];
            }
            catch (CosmosException e)
            {
                Logger.LogError(e,
                    "LoadItemsAsync: CosmosException {ErrorCode} {StatusCode} {Message}",
                    e.StatusCode,
                    e.SubStatusCode,
                    e.Message);
                throw;
            }
        }

        public override async Task<IEnumerable<T>> LoadItemsAsync<T>(Expression<Func<T, bool>> where,
            CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("LoadItemsAsync: {EntityType} with where", typeof(T));

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var iter = _provider.Container
                        .GetItemLinqQueryable<T>(true)
                        .Where(x => x.EntityType == TypeKeyCache.GetEntityTypeKey<T>())
                        .Where(where)
                        .ToFeedIterator()
                    ;

                var results = new List<T>();

                while (iter.HasMoreResults)
                    foreach (var entity in await iter.ReadNextAsync(cancellationToken))
                        results.Add(entity);

                return results;
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning("LoadItemsAsync: Items not found  {ErrorCode} {StatusCode} {Message}",
                    nfex.StatusCode,
                    nfex.SubStatusCode,
                    nfex.Message);
                return new T[0];
            }
            catch (CosmosException e)
            {
                Logger.LogError(e,
                    "LoadItemsAsync: CosmosException {ErrorCode} {StatusCode} {Message}",
                    e.StatusCode,
                    e.SubStatusCode,
                    e.Message);
                throw;
            }
        }

    
        protected override async Task InternalBulkInsertItemsAsync<T>(T[] items, CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("InternalBulkInsertItemsAsync: {EntityType} {EntityCount}", typeof(T), items.Length);


            var client = _provider.BulkClient;
            var database = client.GetDatabase(_provider.Options.DatabaseId);
            var container = database.GetContainer(_provider.Options.CollectionId);

            var concurrentWorkers = _provider.Options.ConcurrentWorkers;

            var workerTasks = new List<Task>(concurrentWorkers);

            var itemsPerWorker = (items.Length / concurrentWorkers) + 1;

            Logger.LogTrace("Initiating process with {ConcurrentWorkers} worker threads for {TotalItems} Items with {ItemsPerWorker} Items per Worker ", concurrentWorkers, items.Length, itemsPerWorker);

            for (var w = 0; w < concurrentWorkers; w++)
            {
                var workerX = w + 1;

                var workerItems = items.GetPaged(itemsPerWorker, workerX);

                var concurrentDocuments = _provider.Options.ConcurrentDocuments;

                Logger.LogTrace("Creating Tasks for Worker {Worker} with {Items} count", workerX, workerItems.Length);

                var workerChunks = (workerItems.Length / concurrentDocuments) + 1;

                Logger.LogTrace("Worker chunks {WorkerChunks} for {ConcurrentDocuments} concurrent docs", workerChunks, concurrentDocuments);

                for (int i = 0;
                    i < workerChunks;
                    i++)
                {
                    var chunkX = i + 1;
                    var chunkItems = workerItems.GetPaged(concurrentDocuments, chunkX);
                    Logger.LogTrace("Worker {Worker} Chunk {Chunk}", w, i);

                    async Task InsertItems()
                    {
                        var tasks = new List<Task>(chunkItems.Length);

                        foreach (var entity in chunkItems)

                            tasks.Add(container.CreateItemAsync(entity, GetPartitionKey<T>(), cancellationToken: cancellationToken)
                                .ContinueWith(task =>
                                {
                                    if (task.Exception != null)
                                    {
                                        AggregateException innerExceptions = task.Exception.Flatten();
                                        var cosmosException = innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) as CosmosException;
                                        if (cosmosException != null)
                                            Logger.LogError("Item {EntityId} failed with status code {StatusCode}", entity.EntityId, cosmosException.StatusCode);
                                        else
                                            Logger.LogError(task.Exception.GetBaseException(), "Error {EntityId}", entity.EntityId);

                                    }
                                    //if (!task.IsCompletedSuccessfully)
                                    //{
                                    //    //        AggregateException innerExceptions = task.Exception.Flatten();
                                    //    //        CosmosException cosmosException = innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) as CosmosException;
                                    //    //        Logger.LogError("Item {EntityId} failed with status code {StatusCode}", entity.EntityId, cosmosException.StatusCode);
                                    //}
                                }, cancellationToken)

                            );

                        await Task.WhenAll(tasks);
                    }

                    workerTasks.Add(InsertItems());
                }
            }

            await Task.WhenAll(workerTasks);
        }

        protected override async Task InternalDeleteAllItemsAsync<T>(CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("InternalDeleteAllItemsAsync: {EntityType}", typeof(T));

            var iter = _provider.Container
                .GetItemLinqQueryable<T>(true)
                .Where(x => x.EntityType == TypeKeyCache.GetEntityTypeKey<T>())
                .ToFeedIterator();

            while (iter.HasMoreResults)
                foreach (var entity in await iter.ReadNextAsync(cancellationToken))
                    try
                    {
                        await _provider.Container.DeleteItemAsync<T>(
                            partitionKey: GetPartitionKey<T>(),
                            id: entity.EntityId,
                            cancellationToken: cancellationToken);
                    }
                    catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
                    {
                        Logger.LogWarning("InternalDeleteItemAsync: Entity with Id not found {EntityId}",
                            entity.EntityId);
                    }
                    catch (CosmosException e)
                    {
                        Logger.LogError(e,
                            "InternalDeleteItemAsync: CosmosException {ErrorCode} {StatusCode} {Message}",
                            e.StatusCode,
                            e.SubStatusCode,
                            e.Message);
                        throw;
                    }
        }

        protected override async Task InternalDeleteItemAsync<T>(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            Logger.LogTrace("InternalDeleteItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _provider.Container.DeleteItemAsync<T>(
                    partitionKey: GetPartitionKey<T>(),
                    id: entityId,
                    cancellationToken: cancellationToken);
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning("InternalDeleteItemAsync: Entity with Id not found {EntityId}", entityId);
            }
            catch (CosmosException e)
            {
                Logger.LogError(e,
                    "InternalDeleteItemAsync: CosmosException {ErrorCode} {StatusCode} {Message}",
                    e.StatusCode,
                    e.SubStatusCode,
                    e.Message);
                throw;
            }
        }

        protected override async Task InternalDeleteItemsAsync<T>(Expression<Func<T, bool>> where,
            CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("InternalDeleteItemsAsync: {EntityType} with where", typeof(T));

            var iter = _provider.Container
                .GetItemLinqQueryable<T>(true)
                .Where(x => x.EntityType == TypeKeyCache.GetEntityTypeKey<T>())
                .Where(where)
                .ToFeedIterator();

            while (iter.HasMoreResults)
                foreach (var entity in await iter.ReadNextAsync(cancellationToken))
                    try
                    {
                        await _provider.Container.DeleteItemAsync<T>(
                            partitionKey: GetPartitionKey<T>(),
                            id: entity.EntityId,
                            cancellationToken: cancellationToken);
                    }
                    catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
                    {
                        Logger.LogWarning("InternalDeleteItemAsync: Entity with Id not found {EntityId}",
                            entity.EntityId);
                    }
                    catch (CosmosException e)
                    {
                        Logger.LogError(e,
                            "InternalDeleteItemAsync: CosmosException {ErrorCode} {StatusCode} {Message}",
                            e.StatusCode,
                            e.SubStatusCode,
                            e.Message);
                        throw;
                    }
        }

        protected override async Task InternalInsertItemAsync<T>(T item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.LogTrace("InternalInsertItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _provider.Container.CreateItemAsync(item, GetPartitionKey<T>(), cancellationToken: cancellationToken);
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.Conflict)
            {
                Logger.LogWarning("InternalInsertItemAsync: Insert Failed, Try Update {EntityId} // {ExceptionMessage}",
                    item.EntityId,
                    nfex.Message);

                if (_provider.Options.ThrowDeveloperError)
                {
                    throw;
                }


                await _provider.Container.UpsertItemAsync(item, GetPartitionKey<T>(), cancellationToken: cancellationToken);
            }
            catch (CosmosException e)
            {
                Logger.LogError(e,
                    "InternalInsertItemAsync: CosmosException {ErrorCode} {StatusCode} {Message}",
                    e.StatusCode,
                    e.SubStatusCode,
                    e.Message);
                throw;
            }
        }

        protected override async Task InternalInsertOrUpdateItemAsync<T>(T item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.LogTrace("InternalInsertOrUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

            cancellationToken.ThrowIfCancellationRequested();

            var exi = await ExistsDocument(item.EntityId, cancellationToken);
            if (exi)
            {
                await UpdateItemAsync(item, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await InsertItemAsync(item, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override async Task<T> InternalLoadItemAsync<T>(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            Logger.LogTrace("InternalLoadItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var res = await _provider.Container.ReadItemAsync<T>(entityId,
                    GetPartitionKey<T>(),
                    cancellationToken: cancellationToken);

                if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                return res.Resource;
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogDebug("InternalLoadItemAsync: Entity with Id not found {EntityId}", entityId);
                return null;
            }
            catch (CosmosException e)
            {
                Logger.LogError(e,
                    "InternalLoadItemAsync CosmosException {ErrorCode} {StatusCode} {Message}",
                    e.StatusCode,
                    e.SubStatusCode,
                    e.Message);
                throw;
            }
        }

        protected override async Task InternalUpdateItemAsync<T>(T item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.LogTrace("InternalUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _provider.Container.UpsertItemAsync(item, GetPartitionKey<T>(), cancellationToken: cancellationToken);
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning("Update Failed, Try Insert {EntityId} // {ExceptionMessage}",
                    item.EntityId,
                    nfex.Message);

                if (_provider.Options.ThrowDeveloperError)
                {
                    throw;
                }


                await _provider.Container.CreateItemAsync(item, GetPartitionKey<T>(), cancellationToken: cancellationToken);
            }
            catch (CosmosException e)
            {
                Logger.LogError(e,
                    "InternalUpdateItemAsync CosmosException {ErrorCode} {StatusCode} {Message}",
                    e.StatusCode,
                    e.SubStatusCode,
                    e.Message);
                throw;
            }
        }

        private async Task<bool> ExistsDocument(string itemEntityId, CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("ExistsDocument: {EntityId}", itemEntityId);

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var query = new QueryDefinition(
                        "select value count(1) from c where c.entity_id = @entityId")
                    .WithParameter("@entityId", itemEntityId);

                var iter = _provider.Container.GetItemQueryIterator<int>(query);

                var x = await iter.ReadNextAsync(cancellationToken);
                var c = x.Resource.First();
                return c == 1;
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning("ExistsDocument: not found  {ErrorCode} {StatusCode} {Message}",
                    nfex.StatusCode,
                    nfex.SubStatusCode,
                    nfex.Message);
                return false;
            }
            catch (CosmosException e)
            {
                Logger.LogError(e,
                    "ExistsDocument: CosmosException {ErrorCode} {StatusCode} {Message}",
                    e.StatusCode,
                    e.SubStatusCode,
                    e.Message);
                throw;
            }
        }

        private PartitionKey GetPartitionKey<T>() where T : Entity
        {
            return new PartitionKey(TypeKeyCache.GetEntityTypeKey<T>());
        }
    }
}