using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
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
            _logger.LogInformation("AzureCosmosDb Options {@Options}", options.Value.CreateSecured());

            Client = new CosmosClientBuilder(Options.EndpointUrl, Options.AuthKey)
                    .WithThrottlingRetryOptions(TimeSpan.FromMinutes(Options.MaxRetryWaitTimeOnThrottledRequestsMinutes),
                        Options.MaxRetryAttemptsOnThrottledRequests)
                    //.AddCustomHandlers(new ThrottlingHandler(_logger))
                    .Build();

            BulkClient = new CosmosClientBuilder(Options.EndpointUrl, Options.AuthKey)
                  //.AddCustomHandlers(new ThrottlingHandler(_logger))
                  .WithThrottlingRetryOptions(TimeSpan.FromMinutes(Options.MaxRetryWaitTimeOnThrottledRequestsMinutes),
                      Options.MaxRetryAttemptsOnThrottledRequests)
                .WithBulkExecution(true)
                .Build();

            Database = Client.GetDatabase(Options.DatabaseId);
            Container = Database.GetContainer(Options.CollectionId);
        }

        public Container Container { get; }

        public Database Database { get; }

        public CosmosClient Client { get; }

        public CosmosClient BulkClient { get; }

        public AzureCosmosDbOptions Options { get; }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            var databaseResponse = await Client.CreateDatabaseIfNotExistsAsync(Options.DatabaseId, Options.OfferThroughput, cancellationToken: cancellationToken).ConfigureAwait(false);
            var database = databaseResponse.Database;

            var readResponse = await database.ReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            await readResponse.Database.CreateContainerIfNotExistsAsync(Options.CollectionId, "/entity_type", cancellationToken: cancellationToken, throughput: Options.OfferThroughput).ConfigureAwait(false);

            if (Options.ScaleOfferThroughput)
            {
                var throughputResponse = await database.ReadThroughputAsync(cancellationToken).ConfigureAwait(false);
                if (throughputResponse.HasValue)

                    if (throughputResponse.Value != Options.OfferThroughput)
                        await database.ReplaceThroughputAsync(Options.OfferThroughput, cancellationToken: cancellationToken).ConfigureAwait(false);
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

                var contRespose = await Container.ReadContainerAsync().ConfigureAwait(false);
                if (contRespose.StatusCode == HttpStatusCode.OK)
                {
                    await Container.DeleteContainerAsync().ConfigureAwait(false);
                }

                var dbResponse = await Database.ReadAsync().ConfigureAwait(false);

                if (dbResponse.StatusCode == HttpStatusCode.OK)
                {
                    await Database.DeleteAsync().ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in Clear Database");
            }
        }
    }
}