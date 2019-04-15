using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SoftwarePioniere.Extensions.Builder;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Extensions.Hosting
{
    public static class SopiBuilderEntityStoreExtensions
    {
        private static string CreateDatabaseId(ISopiBuilder builder)
        {

            if (string.IsNullOrEmpty(builder.Options.AppContext))
            {
                return $"{builder.Options.AppId}-{builder.Version}".Replace(".", "-").Replace(" ", "");
            }
            else
            {
                return $"{builder.Options.AppContext}-{builder.Version}".Replace(".", "-").Replace(" ", "");
            }
        }

        public static ISopiBuilder AddEntityStore(this ISopiBuilder builder)
        {
            var config = builder.Config;


            Console.WriteLine($"Fliegel 365 EntityStore Config Value: {builder.Options.EntityStore}");
            switch (builder.Options.EntityStore)
            {
                case SopiOptions.EntityStoreMongoDb:

                    Console.WriteLine("Adding MongoDb EntityStore");
                    builder.AddMongoDbEntityStore(c => config.Bind("MongoDb", c));
                    builder.Services.PostConfigure<MongoDbOptions>(options =>
                    {
                        Console.WriteLine("Configuring MongoDb EntityStore Options");
                        options.DatabaseId = CreateDatabaseId(builder);
                        Console.WriteLine($"New MongoDb DatabaseId {options.DatabaseId}");

                    });
                    break;


                case SopiOptions.EntityStoreAzureCosmosDb:
                    Console.WriteLine("Adding AzureCosmosDb EntityStore");
                    builder.AddAzureCosmosDbEntityStore(c => config.Bind("AzureCosmosDb", c));
                    builder.Services.PostConfigure<AzureCosmosDbOptions>(options =>
                    {
                        if (string.IsNullOrEmpty(options.CollectionId))
                        {
                            Console.WriteLine("Configuring AzureCosmosDb EntityStore Options");
                            options.CollectionId = CreateDatabaseId(builder);
                            Console.WriteLine($"New AzureCosmosDb CollectionId {options.CollectionId}");
                        }
                    });
                    break;

                default:
                    Console.WriteLine("Adding InMemory EntityStore");
                    builder.AddInMemoryEntityStore();
                    break;

            }

            return builder;

        }
        public static ISopiBuilder AddInMemoryEntityStore(this ISopiBuilder builder)
        {
            builder.Services.AddInMemoryEntityStore();

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.EntityStore = SopiOptions.InMemory;
            });

            return builder;
        }

        public static ISopiBuilder AddAzureCosmosDbEntityStore(this ISopiBuilder builder)
        {
            return builder.AddAzureCosmosDbEntityStore(_ => { });
        }


        public static ISopiBuilder AddAzureCosmosDbEntityStore(this ISopiBuilder builder,
            Action<AzureCosmosDbOptions> configureOptions)
        {
            builder.Services.AddAzureCosmosDbEntityStore(configureOptions);

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.EntityStore = SopiOptions.EntityStoreAzureCosmosDb;
            });

            var options = new AzureCosmosDbOptions();
            configureOptions(options);
            options.CollectionId = CreateDatabaseId(builder);

            builder.GetHealthChecksBuilder()
                .AddDocumentDb(setup =>
                    {
                        setup.PrimaryKey = options.AuthKey;
                        setup.UriEndpoint = options.EndpointUrl;
                    },
                    "azurecosmosdb-entitystore");

            return builder;
        }

        public static ISopiBuilder AddMongoDbEntityStore(this ISopiBuilder builder)
        {
            return builder.AddMongoDbEntityStore(_ => { });
        }

        public static ISopiBuilder AddMongoDbEntityStore(this ISopiBuilder builder,
            Action<MongoDbOptions> configureOptions)
        {
            builder.Services.AddMongoDbEntityStore(configureOptions);

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.EntityStore = SopiOptions.EntityStoreMongoDb;
            });


            var options = new MongoDbOptions();
            configureOptions(options);
            options.DatabaseId = CreateDatabaseId(builder);

            var url = new MongoServerAddress(options.Server, options.Port);
            var settings = new MongoClientSettings
            {
                Server = url,
                ConnectTimeout = TimeSpan.FromSeconds(0.5),
                HeartbeatTimeout = TimeSpan.FromSeconds(0.5),
                ServerSelectionTimeout = TimeSpan.FromSeconds(0.5),
                SocketTimeout = TimeSpan.FromSeconds(0.5)
            };

            builder.GetHealthChecksBuilder()
                .AddMongoDb(settings,
                    options.DatabaseId,
                    "mongodb-entitystore");


            return builder;
        }

    }
}