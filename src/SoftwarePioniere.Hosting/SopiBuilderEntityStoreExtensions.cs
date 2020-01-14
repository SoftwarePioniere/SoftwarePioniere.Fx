using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.AzureCosmosDb;
using SoftwarePioniere.Builder;
using SoftwarePioniere.MongoDb;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderEntityStoreExtensions
    {

        public static ISopiBuilder AddEntityStore(this ISopiBuilder builder)
        {
            var config = builder.Config;


            builder.Log($"EntityStore Config Value: {builder.Options.EntityStore}");
            switch (builder.Options.EntityStore)
            {
                case SopiOptions.EntityStoreMongoDb:

                    builder.Log("Adding MongoDb EntityStore");
                    builder.AddMongoDbEntityStore(c => config.Bind("MongoDb", c));
                    builder.Services.PostConfigure<MongoDbOptions>(options =>
                    {
                        if (string.IsNullOrEmpty(options.DatabaseId))
                        {
                            builder.Log("Configuring MongoDb EntityStore Options");
                            options.DatabaseId = builder.Options.CreateDatabaseId();
                            builder.Log($"New MongoDb DatabaseId {options.DatabaseId}");
                        }
                    });
                    break;


                case SopiOptions.EntityStoreAzureCosmosDb:
                    builder.Log("Adding AzureCosmosDb EntityStore");
                    builder.AddAzureCosmosDbEntityStore(c => config.Bind("AzureCosmosDb", c));
                    builder.Services.PostConfigure<AzureCosmosDbOptions>(options =>
                    {
                        if (string.IsNullOrEmpty(options.CollectionId))
                        {
                            builder.Log("Configuring AzureCosmosDb EntityStore Options");
                            options.CollectionId = builder.Options.CreateDatabaseId();
                            builder.Log($"New AzureCosmosDb CollectionId {options.CollectionId}");
                        }
                    });
                    break;

                default:
                    builder.Log("Adding InMemory EntityStore");
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

        public static ISopiBuilder AddAzureCosmosDbEntityStore(this ISopiBuilder builder,
               Action<AzureCosmosDbOptions> configureOptions)
        {
            builder.Services.AddAzureCosmosDbEntityStore(configureOptions);

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.EntityStore = SopiOptions.EntityStoreAzureCosmosDb;
            });

            //var options = new AzureCosmosDbOptions();
            //configureOptions(options);
            //options.CollectionId = CreateDatabaseId(builder);

            //builder.GetHealthChecksBuilder()
            //    .AddDocumentDb(setup =>
            //        {
            //            setup.PrimaryKey = options.AuthKey;
            //            setup.UriEndpoint = options.EndpointUrl;
            //        },
            //        "azurecosmosdb-entitystore");

            return builder;
        }

        public static ISopiBuilder AddMongoDbEntityStore(this ISopiBuilder builder,
            Action<MongoDbOptions> configureOptions)
        {

            builder.Services
                .AddMongoDbEntityStoreOptions(configureOptions)
                .AddMongoDbEntityStore();

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.EntityStore = SopiOptions.EntityStoreMongoDb;
            });


            //var options = new MongoDbOptions();
            //configureOptions(options);
            //options.DatabaseId = builder.Options.CreateDatabaseId();

            //var url = new MongoServerAddress(options.Server, options.Port);
            //var settings = new MongoClientSettings
            //{
            //    Server = url,
            //    ConnectTimeout = TimeSpan.FromSeconds(0.5),
            //    HeartbeatTimeout = TimeSpan.FromSeconds(0.5),
            //    ServerSelectionTimeout = TimeSpan.FromSeconds(0.5),
            //    SocketTimeout = TimeSpan.FromSeconds(0.5)
            //};

            //builder.GetHealthChecksBuilder()
            //    .AddMongoDb(settings,
            //        options.DatabaseId,
            //        "mongodb-entitystore");


            return builder;
        }

    }
}