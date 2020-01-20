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
        private readonly CosmosClient _bulkClient;
        private readonly ILogger _logger;
        private bool _isInitialized;
        private readonly Container _container;
        private readonly CosmosClient _client;

        public AzureComsosDbConnectionProvider3(ILoggerFactory loggerFactory,
            IOptions<AzureCosmosDbOptions> options)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger(GetType());

            Options = options.Value;
            _logger.LogInformation("AzureCosmosDb Options {@Options}", options.Value.CreateSecured());

            _client = new CosmosClientBuilder(Options.EndpointUrl, Options.AuthKey)
                .WithThrottlingRetryOptions(TimeSpan.FromMinutes(Options.MaxRetryWaitTimeOnThrottledRequestsMinutes),
                    Options.MaxRetryAttemptsOnThrottledRequests)
                //.AddCustomHandlers(new ThrottlingHandler(_logger))
                .Build();

            _bulkClient = new CosmosClientBuilder(Options.EndpointUrl, Options.AuthKey)
                //.AddCustomHandlers(new ThrottlingHandler(_logger))
                .WithThrottlingRetryOptions(TimeSpan.FromMinutes(Options.MaxRetryWaitTimeOnThrottledRequestsMinutes),
                    Options.MaxRetryAttemptsOnThrottledRequests)
                .WithBulkExecution(true)
                .Build();

            Database = Client.GetDatabase(Options.DatabaseId);
            _container = Database.GetContainer(Options.CollectionId);
        }

        public CosmosClient BulkClient
        {
            get
            {
                AssertInitialized();
                return _bulkClient;
            }
        }

        public CosmosClient Client
        {
            get
            {
                AssertInitialized();
                return _client;
            }
        }

        public Container Container
        {
            get
            {
                AssertInitialized();
                return _container;
            }
        }

        private Database Database { get; }

        public AzureCosmosDbOptions Options { get; }

        public void Dispose()
        {
            Client?.Dispose();
        }


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

                {
                    if (throughputResponse.Value != Options.OfferThroughput)
                    {
                        await database.ReplaceThroughputAsync(Options.OfferThroughput, cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            _isInitialized = true;
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

        private void AssertInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Initialize First");
            }
        }
    }
}