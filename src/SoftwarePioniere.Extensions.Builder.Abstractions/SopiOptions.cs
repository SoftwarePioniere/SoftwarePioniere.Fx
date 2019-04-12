namespace SoftwarePioniere.Extensions.Builder
{

    public class SopiOptions
    {
        public const string AuthAuth0 = "Auth0";
        public const string AuthAzureAd = "AzureAd";
        public const string MessageBusRedis = "Redis";
        //public const string MessageBusRabbitMq = "RabbitMQ";
        public const string EntityStoreMongoDb = "MongoDb";
        public const string EntityStoreAzureCosmosDb = "AzureCosmosDb";
        public const string CacheRedis = "Redis";
        public const string InMemory = "InMemory";
        public const string StorageFolder = "Folder";
        public const string StorageAzureStorage = "AzureStorage";
        public const string DomainEventStoreEventStore = "EventStore";

        public int TaskQueueMaxItems { get; set; } = int.MaxValue;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string AppId { get; set; } //= "sopi-test";
        public string AppContext { get; set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string AllowedOrigins { get; set; } = "*"; //"http://localhost:4200;http://localhost:8100;https://fliegel365-dev-start.azurewebsites.net/";
        public string Auth { get; set; }
        public string MessageBus { get; set; }
        public string EntityStore { get; set; }
        public string DomainEventStore { get; set; }
        public string CacheClient { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool AllowDevMode { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Storage { get; set; }
        public string WebSocketPaths { get; set; }
        
    }
}