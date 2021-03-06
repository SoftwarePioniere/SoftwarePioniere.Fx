﻿using System;
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


        private Lazy<IMongoDatabase> _database;
        private Lazy<IMongoDatabase> _databaseInsert;
        private Lazy<IMongoDatabase> _databaseLoadItem;
        private Lazy<IMongoDatabase> _databaseLoadItems;

        private bool _isInitialized;

        static MongoDbConnectionProvider()
        {
            // set your options on this line
            var serializer = new DateTimeSerializer(DateTimeKind.Utc, BsonType.Document);
            BsonSerializer.RegisterSerializer(typeof(DateTime), serializer);
        }

        public MongoDbConnectionProvider(ILoggerFactory loggerFactory, IOptions<MongoDbOptions> options)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger(GetType());

            KeyCache = new TypeKeyCache();

            Options = options.Value;

            _logger.LogInformation("MongoDb Options {@Options}", options.Value.CreateSecured());

            //InitClient();
            InitDatabase();
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

        public Lazy<IMongoDatabase> Database
        {
            get
            {
                AssertInitialized();
                return _database;
            }
            private set => _database = value;
        }

        //public Lazy<IMongoClient> Client2 { get; private set; }

        public Lazy<IMongoDatabase> DatabaseInsert
        {
            get
            {
                AssertInitialized();
                return _databaseInsert;
            }
            private set => _databaseInsert = value;
        }

        //public Lazy<IMongoClient> ClientLoadItem { get; private set; }

        public Lazy<IMongoDatabase> DatabaseLoadItem
        {
            get
            {
                AssertInitialized();
                return _databaseLoadItem;
            }
            private set => _databaseLoadItem = value;
        }


        //public Lazy<IMongoClient> Client3 { get; private set; }

        public Lazy<IMongoDatabase> DatabaseLoadItems
        {
            get
            {
                AssertInitialized();
                return _databaseLoadItems;
            }
            private set => _databaseLoadItems = value;
        }

        public TypeKeyCache KeyCache { get; }

        //private readonly Uri _collectionUri;

        public MongoDbOptions Options { get; set; }


        public async Task ClearDatabaseAsync()
        {
            _logger.LogInformation("Clear Database");

            await CreateClient().DropDatabaseAsync(Options.DatabaseId).ConfigureAwait(false);
            _logger.LogInformation("Reinit Client");
            InitDatabase();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _isInitialized = true;
            await CheckDatabaseExistsAsync().ConfigureAwait(false);
        }

        private void AssertInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Initialize First");
            }
        }


        public async Task<bool> CheckDatabaseExistsAsync()
        {
            _logger.LogTrace(nameof(CheckDatabaseExistsAsync));

            var client = CreateClient();

            var databases = await client.ListDatabasesAsync().ConfigureAwait(false);

            while (await databases.MoveNextAsync().ConfigureAwait(false))
            {
                var items = databases.Current.ToArray();
                var c = items.Count(x => x["name"] == Options.DatabaseId) > 0;

                if (c)
                {
                    return true;
                }
            }

            return false;
        }

        public IMongoClient CreateClient()
        {
            var url = new MongoServerAddress(Options.Server, Options.Port);
            var settings = new MongoClientSettings
            {
                Server = url
            };

            if (!string.IsNullOrEmpty(Options.UserName) && !string.IsNullOrEmpty(Options.Password))
            {
                settings.Credential =
                    MongoCredential.CreateCredential(Options.DatabaseId, Options.UserName, Options.Password);
            }


            if (Options.ClusterConfigurator != null)
            {
                settings.ClusterConfigurator = Options.ClusterConfigurator;
            }

            var client = new MongoClient(settings);
            return client;
        }

        public IMongoCollection<T> GetCol<T>() where T : Entity
        {
            return Database.Value.GetCollection<T>(KeyCache.GetEntityTypeKey<T>());
        }

        public IMongoCollection<T> GetColInsert<T>() where T : Entity
        {
            return DatabaseInsert.Value.GetCollection<T>(KeyCache.GetEntityTypeKey<T>());
        }

        public IMongoCollection<T> GetColLoadItem<T>() where T : Entity
        {
            return DatabaseLoadItem.Value.GetCollection<T>(KeyCache.GetEntityTypeKey<T>());
        }

        public IMongoCollection<T> GetColLoadItems<T>() where T : Entity
        {
            return DatabaseLoadItems.Value.GetCollection<T>(KeyCache.GetEntityTypeKey<T>());
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
    }
}