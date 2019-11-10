using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.AzureCosmosDb
{
    public class AzureCosmodbEntityStore3 : EntityStoreBase<AzureCosmosDbOptions>
    {
        private readonly AzureComsosDbConnectionProvider3 _provider;


        public AzureCosmodbEntityStore3(AzureCosmosDbOptions options,
            AzureComsosDbConnectionProvider3 provider) : base(options)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public override async Task<T[]> LoadItemsAsync<T>(CancellationToken token = default)
        {
            Logger.LogTrace("LoadItemsAsync: {EntityType}", typeof(T));

            token.ThrowIfCancellationRequested();

            try
            {
                var iter = _provider.Container
                        .GetItemLinqQueryable<T>(true)
                        .Where(x => x.EntityType == TypeKeyCache.GetEntityTypeKey<T>())
                        .ToFeedIterator()
                    ;

                var results = new List<T>();

                while (iter.HasMoreResults)
                {
                    foreach (var entity in await iter.ReadNextAsync(token))
                    {
                        results.Add(entity);
                    }
                }

                return results.ToArray();
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning("LoadItemsAsync: Items not found  {ErrorCode} {StatusCode} {Message}", nfex.StatusCode, nfex.SubStatusCode, nfex.Message);
                return new T[0];
            }
            catch (CosmosException e)
            {
                Logger.LogError(e, "LoadItemsAsync: CosmosException {ErrorCode} {StatusCode} {Message}", e.StatusCode, e.SubStatusCode, e.Message);
                throw;
            }

        }

        public override async Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, CancellationToken token = default)
        {

            Logger.LogTrace("LoadItemsAsync: {EntityType} with where", typeof(T));

            token.ThrowIfCancellationRequested();

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
                {
                    foreach (var entity in await iter.ReadNextAsync(token))
                    {
                        results.Add(entity);
                    }
                }

                return results.ToArray();
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning("LoadItemsAsync: Items not found  {ErrorCode} {StatusCode} {Message}", nfex.StatusCode, nfex.SubStatusCode, nfex.Message);
                return new T[0];
            }
            catch (CosmosException e)
            {
                Logger.LogError(e, "LoadItemsAsync: CosmosException {ErrorCode} {StatusCode} {Message}", e.StatusCode, e.SubStatusCode, e.Message);
                throw;
            }
        }

        public override Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override async Task InternalDeleteItemAsync<T>(string entityId, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            Logger.LogTrace("InternalDeleteItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            token.ThrowIfCancellationRequested();

            try
            {
                await _provider.Container.DeleteItemAsync<T>(
                    partitionKey: GetPartitionKey<T>(),
                    id: entityId, cancellationToken: token);
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning("InternalDeleteItemAsync: Entity with Id not found {EntityId}", entityId);
            }
            catch (CosmosException e)
            {
                Logger.LogError(e, "InternalDeleteItemAsync: CosmosException {ErrorCode} {StatusCode} {Message}", e.StatusCode, e.SubStatusCode, e.Message);
                throw;
            }
        }

        private PartitionKey GetPartitionKey<T>() where T : Entity
        {
            return new PartitionKey(TypeKeyCache.GetEntityTypeKey<T>());
        }

        protected override async Task InternalDeleteItemsAsync<T>(Expression<Func<T, bool>> where, CancellationToken token = default)
        {
            Logger.LogTrace("InternalDeleteItemsAsync: {EntityType} with where", typeof(T));

            var iter = _provider.Container
                .GetItemLinqQueryable<T>(true)
                .Where(x => x.EntityType == TypeKeyCache.GetEntityTypeKey<T>())
                .Where(where)
                .ToFeedIterator();

            while (iter.HasMoreResults)
            {
                foreach (var entity in await iter.ReadNextAsync(token))
                {
                    try
                    {
                        await _provider.Container.DeleteItemAsync<T>(
                            partitionKey: GetPartitionKey<T>(),
                            id: entity.EntityId, cancellationToken: token);
                    }
                    catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
                    {
                        Logger.LogWarning("InternalDeleteItemAsync: Entity with Id not found {EntityId}", entity.EntityId);
                    }
                    catch (CosmosException e)
                    {
                        Logger.LogError(e, "InternalDeleteItemAsync: CosmosException {ErrorCode} {StatusCode} {Message}", e.StatusCode, e.SubStatusCode, e.Message);
                        throw;
                    }
                }
            }
        }

        protected override async Task InternalDeleteAllItemsAsync<T>(CancellationToken token = default)
        {
            Logger.LogTrace("InternalDeleteAllItemsAsync: {EntityType}", typeof(T));

            var iter = _provider.Container
                .GetItemLinqQueryable<T>(true)
                .Where(x => x.EntityType == TypeKeyCache.GetEntityTypeKey<T>())
                .ToFeedIterator();

            while (iter.HasMoreResults)
            {
                foreach (var entity in await iter.ReadNextAsync(token))
                {
                    try
                    {
                        await _provider.Container.DeleteItemAsync<T>(
                            partitionKey: GetPartitionKey<T>(),
                            id: entity.EntityId, cancellationToken: token);
                    }
                    catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
                    {
                        Logger.LogWarning("InternalDeleteItemAsync: Entity with Id not found {EntityId}", entity.EntityId);
                    }
                    catch (CosmosException e)
                    {
                        Logger.LogError(e, "InternalDeleteItemAsync: CosmosException {ErrorCode} {StatusCode} {Message}", e.StatusCode, e.SubStatusCode, e.Message);
                        throw;
                    }
                }
            }
        }

        protected override async Task InternalInsertItemAsync<T>(T item, CancellationToken token = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.LogTrace("InternalInsertItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

            token.ThrowIfCancellationRequested();

            try
            {
                await _provider.Container.CreateItemAsync(item, GetPartitionKey<T>(), cancellationToken: token);
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.Conflict)
            {
                Logger.LogWarning("InternalInsertItemAsync: Insert Failed, Try Update {EntityId} // {ExceptionMessage}", item.EntityId, nfex.Message);
                await _provider.Container.UpsertItemAsync(item, GetPartitionKey<T>(), cancellationToken: token);
            } 
            catch (CosmosException e)
            {
                Logger.LogError(e, "InternalInsertItemAsync: CosmosException {ErrorCode} {StatusCode} {Message}", e.StatusCode, e.SubStatusCode, e.Message);
                throw;
            }
        }

        protected override async Task InternalBulkInsertItemsAsync<T>(T[] items, CancellationToken token = default)
        {
            Logger.LogTrace("InternalBulkInsertItemsAsync: {EntityType} {EntityCount}", typeof(T), items.Length);


            foreach (var item in items)
            {
                token.ThrowIfCancellationRequested();
                await _provider.Container.CreateItemAsync(item, GetPartitionKey<T>(), cancellationToken: token);
            }


            //foreach (var item in items)
            //{
            //    await _provider.Container.CreateItemAsync(item, GetPartitionKey<T>(), cancellationToken: token);
            //}

            //var endpoint = new Uri(_provider.Options.EndpointUrl);
            //var policy = new ConnectionPolicy
            //{
            //    ConnectionMode = Microsoft.Azure.Documents.Client.ConnectionMode.Gateway,
            //    ConnectionProtocol = Protocol.Https,
            //    MaxConnectionLimit = 100,
            //    RetryOptions = { MaxRetryWaitTimeInSeconds = 0, MaxRetryAttemptsOnThrottledRequests = 0 }
            //};

            //// Set retries to 0 to pass complete control to bulk executor.

            //using (var client = new DocumentClient(endpoint, Options.AuthKey, policy))
            //{

            //    var collection = client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(Options.DatabaseId))
            //        .Where(c => c.Id == Options.CollectionId)
            //        .ToArray()
            //        .SingleOrDefault();

            //    IBulkExecutor bulkExecutor = new BulkExecutor(client, collection);
            //    await bulkExecutor.InitializeAsync();

            //    var bulkImportResponse = await bulkExecutor.BulkImportAsync(
            //        documents: items,
            //        enableUpsert: true,
            //        disableAutomaticIdGeneration: true,
            //        maxConcurrencyPerPartitionKeyRange: null,
            //        maxInMemorySortingBatchSize: null,
            //        cancellationToken: token);

            //    Logger.LogTrace(
            //        "BulkImportAsync: Imported: {NumberOfDocumentsImported} / RequestUnits: {RequestCharge} / TimeTaken {TotalTimeTaken}",
            //        bulkImportResponse.NumberOfDocumentsImported,
            //        bulkImportResponse.TotalRequestUnitsConsumed,
            //        bulkImportResponse.TotalTimeTaken);

            //    if (bulkImportResponse.BadInputDocuments != null && bulkImportResponse.BadInputDocuments.Any())
            //    {
            //        Logger.LogWarning("BulkImport Bad Documents");
            //        foreach (var o in bulkImportResponse.BadInputDocuments)
            //        {
            //            Logger.LogWarning("BulkImport Bad Doc {@doc}", o);
            //        }

            //        throw new InvalidOperationException("Bulk Import Bad Documents");
            //    }
            //}
        }

        protected override async Task InternalInsertOrUpdateItemAsync<T>(T item, CancellationToken token = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.LogTrace("InternalInsertOrUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

            token.ThrowIfCancellationRequested();

            var exi = await ExistsDocument(item.EntityId, token);
            if (exi)
            {
                await UpdateItemAsync(item, token).ConfigureAwait(false);
            }
            else
            {
                await InsertItemAsync(item, token).ConfigureAwait(false);
            }
        }

        private async Task<bool> ExistsDocument(string itemEntityId, CancellationToken token = default)
        {
            Logger.LogTrace("ExistsDocument: {EntityId}", itemEntityId);

            token.ThrowIfCancellationRequested();

            try
            {

                var query = new QueryDefinition(
                        "select value count(1) from c where c.entity_id = @entityId")
                    .WithParameter("@entityId", itemEntityId);

                var iter = _provider.Container.GetItemQueryIterator<int>(query);

                var x = await iter.ReadNextAsync(token);
                var c = x.Resource.First();
                return c == 1;
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning("ExistsDocument: not found  {ErrorCode} {StatusCode} {Message}", nfex.StatusCode, nfex.SubStatusCode, nfex.Message);
                return false;
            }
            catch (CosmosException e)
            {
                Logger.LogError(e, "ExistsDocument: CosmosException {ErrorCode} {StatusCode} {Message}", e.StatusCode, e.SubStatusCode, e.Message);
                throw;
            }
        }

        protected override async Task<T> InternalLoadItemAsync<T>(string entityId, CancellationToken token = default)
        {

            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            Logger.LogTrace("InternalLoadItemAsync: {EntityType} {EntityId}", typeof(T), entityId);

            token.ThrowIfCancellationRequested();

            try
            {
                
                var res = await _provider.Container.ReadItemAsync<T>(entityId, GetPartitionKey<T>(), cancellationToken: token);

                if (res.StatusCode == HttpStatusCode.NotFound)
                    return null;

                return res.Resource;
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogDebug("InternalLoadItemAsync: Entity with Id not found {EntityId}", entityId);
                return null;
            }
            catch (CosmosException e)
            {
                Logger.LogError(e, "InternalLoadItemAsync CosmosException {ErrorCode} {StatusCode} {Message}", e.StatusCode, e.SubStatusCode, e.Message);
                throw;
            }
        }

        protected override async Task InternalUpdateItemAsync<T>(T item, CancellationToken token = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.LogTrace("InternalUpdateItemAsync: {EntityType} {EntityId}", typeof(T), item.EntityId);

            token.ThrowIfCancellationRequested();

            try
            {
                await _provider.Container.UpsertItemAsync(item, GetPartitionKey<T>(), cancellationToken: token);
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning("Update Failed, Try Insert {EntityId} // {ExceptionMessage}", item.EntityId, nfex.Message);
                await _provider.Container.CreateItemAsync(item, GetPartitionKey<T>(), cancellationToken: token);
            }
            catch (CosmosException e)
            {
                Logger.LogError(e, "InternalUpdateItemAsync CosmosException {ErrorCode} {StatusCode} {Message}", e.StatusCode, e.SubStatusCode, e.Message);
                throw;
            }
        }
    }
}
