using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using SoftwarePioniere.Extensions.Builder;
using SoftwarePioniere.ReadModel;
using StackExchange.Redis;

namespace SoftwarePioniere.Extensions.Hosting
{
    public static class CheckSystemStateExtensions
    {
       
        public static void CheckSystemState(this IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetStartupLogger();

            var options = serviceProvider.GetRequiredService<IOptions<SopiOptions>>().Value;

            if (options.CacheClient == SopiOptions.CacheRedis ||
                options.MessageBus == SopiOptions.MessageBusRedis)
            {
                logger.LogInformation("Checking Redis Connection");

                Policy.Handle<Exception>()
                    .WaitAndRetry(new[]
                    {
                       TimeSpan.FromSeconds(1),
                       TimeSpan.FromSeconds(3),
                       TimeSpan.FromSeconds(5),
                       TimeSpan.FromSeconds(10),
                       TimeSpan.FromSeconds(20)
                    }).Execute(() =>
                    {
                        var con = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
                        if (con.IsConnected)
                        {
                            logger.LogInformation("Redis Is Connected..");
                        }
                        else
                        {
                            logger.LogError("Redis is not connected");
                            throw new InvalidOperationException("Redis Cache is not connected");
                        }
                    });
            }

            if (options.EntityStore == SopiOptions.EntityStoreAzureCosmosDb)
            {
                logger.LogInformation("Checking AzureCosmosDb Connection");

                Policy.Handle<Exception>()
                    .WaitAndRetry(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(3),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(20)
                    }).Execute(() =>
                    {
                        var con = serviceProvider.GetRequiredService<AzureCosmosDbConnectionProvider>();

                        logger.LogInformation("ServiceEndpoint: {ServiceEndpoint}", con.Client.Value.ServiceEndpoint);

                        if (con.CheckDatabaseExists())
                        {
                            logger.LogInformation("AzureCosmosDb Is Connected..");
                        }
                        else
                        {
                            logger.LogError("AzureCosmosDb is not connected");
                            throw new InvalidOperationException("AzureCosmosDb is not connected");
                        }
                    });
            }

            if (options.EntityStore == SopiOptions.EntityStoreMongoDb)
            {
                logger.LogInformation("Checking MongoDb Connection");


                Policy.Handle<Exception>()
                    .WaitAndRetry(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(3),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(20)
                    }).Execute(() =>
                    {
                        var con = serviceProvider.GetRequiredService<MongoDbConnectionProvider>();

                        try
                        {
                            var dbs = con.Client.Value.ListDatabasesAsync().Result;
                            dbs.MoveNextAsync().Wait();
                            logger.LogInformation("MongoDb Is Connected..", dbs.Current.ToArray().Length);

                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "MongoDb is not connected");
                            throw new InvalidOperationException("MongoDb is not connected");
                        }
                    });
            }
        }
    }
}
