using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.Documents.Client;
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

        public override async Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, CancellationToken token = default)
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

        public override Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task InternalDeleteItemAsync<T>(string entityId, CancellationToken token = default)
        {
            return _provider.Container.DeleteItemAsync<T>(
                partitionKey: GetPartitionKey<T>(),
                id: entityId, cancellationToken: token);

        }

        private PartitionKey GetPartitionKey<T>() where T : Entity
        {
            return new PartitionKey(TypeKeyCache.GetEntityTypeKey<T>());
        }

        protected override async Task InternalDeleteItemsAsync<T>(Expression<Func<T, bool>> where, CancellationToken token = default)
        {
            var iter = _provider.Container
                .GetItemLinqQueryable<T>(true)
                .Where(x => x.EntityType == TypeKeyCache.GetEntityTypeKey<T>())
                .Where(where)
                .ToFeedIterator();

            while (iter.HasMoreResults)
            {
                foreach (var entity in await iter.ReadNextAsync(token))
                {
                    await _provider.Container.DeleteItemAsync<T>(entity.EntityId, GetPartitionKey<T>(), cancellationToken: token);
                }
            }
        }

        protected override async Task InternalDeleteAllItemsAsync<T>(CancellationToken token = default)
        {
            var iter = _provider.Container
                .GetItemLinqQueryable<T>(true)
                .Where(x => x.EntityType == TypeKeyCache.GetEntityTypeKey<T>())
                .ToFeedIterator();

            while (iter.HasMoreResults)
            {
                foreach (var entity in await iter.ReadNextAsync(token))
                {
                    await _provider.Container.DeleteItemAsync<T>(entity.EntityId, GetPartitionKey<T>(), cancellationToken: token);
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
                Logger.LogWarning("Insert Failed, Try Update {EntityId} // {ExceptionMessage}", item.EntityId, nfex.Message);
                await _provider.Container.UpsertItemAsync(item, GetPartitionKey<T>(), cancellationToken: token);
            }
        }

        protected override async Task InternalBulkInsertItemsAsync<T>(T[] items, CancellationToken token = default)
        {
            //foreach (var item in items)
            //{
            //    await _provider.Container.CreateItemAsync(item, GetPartitionKey<T>(), cancellationToken: token);
            //}

            var endpoint = new Uri(_provider.Options.EndpointUrl);
            var policy = new ConnectionPolicy
            {
                ConnectionMode = Microsoft.Azure.Documents.Client.ConnectionMode.Gateway,
                ConnectionProtocol = Protocol.Https,
                MaxConnectionLimit = 100,
                RetryOptions = { MaxRetryWaitTimeInSeconds = 0, MaxRetryAttemptsOnThrottledRequests = 0 }
            };

            // Set retries to 0 to pass complete control to bulk executor.

            using (var client = new DocumentClient(endpoint, Options.AuthKey, policy))
            {

                var collection = client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(Options.DatabaseId))
                    .Where(c => c.Id == Options.CollectionId)
                    .ToArray()
                    .SingleOrDefault();

                IBulkExecutor bulkExecutor = new BulkExecutor(client, collection);
                await bulkExecutor.InitializeAsync();

                var bulkImportResponse = await bulkExecutor.BulkImportAsync(
                    documents: items,
                    enableUpsert: true,
                    disableAutomaticIdGeneration: true,
                    maxConcurrencyPerPartitionKeyRange: null,
                    maxInMemorySortingBatchSize: null,
                    cancellationToken: token);

                Logger.LogTrace(
                    "BulkImportAsync: Imported: {NumberOfDocumentsImported} / RequestUnits: {RequestCharge} / TimeTaken {TotalTimeTaken}",
                    bulkImportResponse.NumberOfDocumentsImported,
                    bulkImportResponse.TotalRequestUnitsConsumed,
                    bulkImportResponse.TotalTimeTaken);

                if (bulkImportResponse.BadInputDocuments != null && bulkImportResponse.BadInputDocuments.Any())
                {
                    Logger.LogWarning("BulkImport Bad Documents");
                    foreach (var o in bulkImportResponse.BadInputDocuments)
                    {
                        Logger.LogWarning("BulkImport Bad Doc {@doc}", o);
                    }

                    throw new InvalidOperationException("Bulk Import Bad Documents");
                }
            }
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
            var query = new QueryDefinition(
                    "select value count(1) from c where c.entity_id = @entityId")
                .WithParameter("@entityId", itemEntityId);
            
            var iter = _provider.Container.GetItemQueryIterator<int>(query);

            var x = await iter.ReadNextAsync(token);
            var c = x.Resource.First();
            return c == 1;
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

                return res.Resource;
            }
            catch (CosmosException nfex) when (nfex.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning("Entity with Id not found {EntityId}", entityId);
                return null;
            }
            catch (CosmosException e)
            {
                Logger.LogError(e, "Error in CosmosDb Load {ErrorCode} {StatusCode} {Message}", e.StatusCode, e.SubStatusCode, e.Message);
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
        }
    }
}
