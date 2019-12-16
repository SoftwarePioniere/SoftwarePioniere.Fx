using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.MongoDb
{

    public class MongoDbConnectionProvider : IEntityStoreConnectionProvider
    {
        private readonly ILogger _logger;

        public TypeKeyCache KeyCache { get; private set; }

        //private readonly Uri _collectionUri;

        public MongoDbOptions Options { get; set; }

        static MongoDbConnectionProvider()
        {
            // set your options on this line
            var serializer = new DateTimeSerializer(DateTimeKind.Utc, BsonType.Document);
            BsonSerializer.RegisterSerializer(typeof(DateTime), serializer);
        }

        public MongoDbConnectionProvider(ILoggerFactory loggerFactory, IOptions<MongoDbOptions> options)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(GetType());

            KeyCache = new TypeKeyCache();

            Options = options.Value;

            _logger.LogInformation("MongoDb Options {@Options}", options.Value.CreateSecured());

            //InitClient();
            InitDatabase();
        }


        public async Task<bool> CheckDatabaseExistsAsync()
        {
            _logger.LogTrace(nameof(CheckDatabaseExistsAsync));

            var client = CreateClient();

            var databases = await client.ListDatabasesAsync();

            while (await databases.MoveNextAsync())
            {
                var items = databases.Current.ToArray();
                var c = (items.Count(x => x["name"] == Options.DatabaseId) > 0);

                if (c)
                    return true;

            }

            return false;
        }

        public IMongoClient CreateClient()
        {
            var url = new MongoServerAddress(Options.Server, Options.Port);
            var settings = new MongoClientSettings
            { Server = url };

            if (Options.ClusterConfigurator != null)
                settings.ClusterConfigurator = Options.ClusterConfigurator;

            var client = new MongoClient(settings);
            return client;
        }


        private void InitDatabase()
        {
            Database = new Lazy<IMongoDatabase>(() =>
            {
                var client = CreateClient();
                return client.GetDatabase(Options.DatabaseId);
            });

            DatabaseInsert = new Lazy<IMongoDatabase>(() =>
            {
                var client = CreateClient();
                return client.GetDatabase(Options.DatabaseId);
            });

            DatabaseLoadItems = new Lazy<IMongoDatabase>(() =>
            {
                var client = CreateClient();
                return client.GetDatabase(Options.DatabaseId);
            });


            DatabaseLoadItem = new Lazy<IMongoDatabase>(() =>
            {
                var client = CreateClient();
                return client.GetDatabase(Options.DatabaseId);
            });
        }

        //private void InitClient()
        //{
        //    Client = new Lazy<IMongoClient>(() =>
        //        {
        //            var client = CreateClient();
        //            return client;
        //        }
        //    );

        //    Client2 = new Lazy<IMongoClient>(() =>
        //        {
        //            var client = CreateClient();
        //            return client;
        //        }
        //    );

        //    Client3 = new Lazy<IMongoClient>(() =>
        //        {
        //            var client = CreateClient();
        //            return client;
        //        }
        //    );

        //    ClientLoadItem = new Lazy<IMongoClient>(() =>
        //        {
        //            var client = CreateClient();
        //            return client;
        //        }
        //    );
        //}

        //public Lazy<IMongoClient> Client { get; private set; }

        public Lazy<IMongoDatabase> Database { get; private set; }

        //public Lazy<IMongoClient> Client2 { get; private set; }

        public Lazy<IMongoDatabase> DatabaseInsert { get; private set; }


        //public Lazy<IMongoClient> Client3 { get; private set; }

        public Lazy<IMongoDatabase> DatabaseLoadItems { get; private set; }

        //public Lazy<IMongoClient> ClientLoadItem { get; private set; }

        public Lazy<IMongoDatabase> DatabaseLoadItem { get; private set; }


        public async Task ClearDatabaseAsync()
        {
            _logger.LogInformation("Clear Database");

            await CreateClient().DropDatabaseAsync(Options.DatabaseId);
            _logger.LogInformation("Reinit Client");
            InitDatabase();
        }

        public IMongoCollection<T> GetCol<T>() where T : Entity
        {
            return Database.Value.GetCollection<T>(KeyCache.GetEntityTypeKey<T>());
        }

        public IMongoCollection<T> GetColInsert<T>() where T : Entity
        {
            return DatabaseInsert.Value.GetCollection<T>(KeyCache.GetEntityTypeKey<T>());
        }

        public IMongoCollection<T> GetColLoadItems<T>() where T : Entity
        {
            return DatabaseLoadItems.Value.GetCollection<T>(KeyCache.GetEntityTypeKey<T>());
        }

        public IMongoCollection<T> GetColLoadItem<T>() where T : Entity
        {
            return DatabaseLoadItem.Value.GetCollection<T>(KeyCache.GetEntityTypeKey<T>());
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {


            await CheckDatabaseExistsAsync();
        }
    }
}
