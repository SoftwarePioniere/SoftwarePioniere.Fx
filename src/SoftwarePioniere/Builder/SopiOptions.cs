// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using System;
using System.Reflection;

namespace SoftwarePioniere.Builder
{

    public class SopiOptions
    {
        public SopiOptions()
        {
            var assembly = Assembly.GetEntryAssembly();
            AppVersion = (assembly ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        public const string AuthAuth0 = "Auth0";
        public const string AuthAzureAd = "AzureAd";
        public const string MessageBusRedis = "Redis";
        public const string MessageBusAzureServiceBus = "AzureServiceBus";
        //public const string MessageBusRabbitMq = "RabbitMQ";
        public const string EntityStoreMongoDb = "MongoDb";
        public const string EntityStoreAzureCosmosDb = "AzureCosmosDb";
        public const string CacheRedis = "Redis";
        public const string InMemory = "InMemory";
        public const string StorageFolder = "Folder";
        public const string StorageAzureStorage = "AzureStorage";
        public const string DomainEventStoreEventStore = "EventStore";

        public int TaskQueueMaxItems { get; set; } = int.MaxValue;

        public string AppId { get; set; } //= "sopi-test";
        public string AppContext { get; set; }
        public string AppVersion { get; set; }

        public string AllowedOrigins { get; set; } = "*"; //"http://localhost:4200;http://localhost:8100;https://fliegel365-dev-start.azurewebsites.net/";
        public string Auth { get; set; } = AuthAuth0;
        public string MessageBus { get; set; }
        public string EntityStore { get; set; }
        public string DomainEventStore { get; set; }
        public string CacheClient { get; set; }
        public bool AllowDevMode { get; set; }
        public string Storage { get; set; }
        public string WebSocketPaths { get; set; }

        public string CreateDatabaseId()
        {
            if (!string.IsNullOrEmpty(AppContext))
                return $"{AppContext}-{AppVersion}".Replace(".", "-").Replace(" ", "").Replace("+", "");

            return $"{AppId}-{AppVersion}".Replace(".", "-").Replace(" ", "").Replace("+", "");

        }

    }
}