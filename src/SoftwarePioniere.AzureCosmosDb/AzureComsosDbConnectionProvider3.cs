using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.AzureCosmosDb
{
    public class AzureComsosDbConnectionProvider3 : IEntityStoreConnectionProvider, IDisposable
    {
        private readonly ILogger _logger;

        public AzureComsosDbConnectionProvider3(ILoggerFactory loggerFactory,
            IOptions<AzureCosmosDbOptions> options)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(GetType());

            Options = options.Value;
            _logger.LogInformation("AzureCosmosDb Connection {Connection}", options);
     
            Client = new CosmosClient(Options.EndpointUrl, Options.AuthKey);
            //BulkClient = new CosmosClient(Options.EndpointUrl, Options.AuthKey, new CosmosClientOptions() { AllowBulkExecution = true });
            Database = Client.GetDatabase(Options.DatabaseId);
            Container = Database.GetContainer(Options.CollectionId);
        }

        public Container Container { get; }

        public Database Database { get; }

        public CosmosClient Client { get; }

        //public CosmosClient BulkClient { get; }
    
        public AzureCosmosDbOptions Options { get; }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            var databaseResponse = await Client.CreateDatabaseIfNotExistsAsync(Options.DatabaseId, Options.OfferThroughput, cancellationToken: cancellationToken);
            var database = databaseResponse.Database;

            var readResponse = await database.ReadAsync(cancellationToken: cancellationToken);
            await readResponse.Database.CreateContainerIfNotExistsAsync(Options.CollectionId, "/entity_type", cancellationToken: cancellationToken, throughput: Options.OfferThroughput);

            if (Options.ScaleOfferThroughput)
            {
                var throughputResponse = await database.ReadThroughputAsync(cancellationToken);
                if (throughputResponse.HasValue)

                    if (throughputResponse.Value != Options.OfferThroughput)
                        await database.ReplaceThroughputAsync(Options.OfferThroughput, cancellationToken: cancellationToken);
            }
        }

        public void Dispose()
        {
            Client?.Dispose();
        }


        public async Task ClearDatabaseAsync()
        {
            try
            {
                
                var contRespose = await Container.ReadContainerAsync();
                if (contRespose.StatusCode == HttpStatusCode.OK)
                {
                    await Container.DeleteContainerAsync();
                }

                var dbResponse = await Database.ReadAsync();

                if (dbResponse.StatusCode == HttpStatusCode.OK)
                {
                    await Database.DeleteAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in Clear Database");
            }
        }
    }
}