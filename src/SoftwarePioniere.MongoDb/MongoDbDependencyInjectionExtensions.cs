using System;
using Foundatio.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.MongoDb;
using SoftwarePioniere.ReadModel;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoDbDependencyInjectionExtensions
    {
     
        public static IServiceCollection AddMongoDbEntityStore(this IServiceCollection services, Action<MongoDbOptions> configureOptions)
        {

            services
                .AddOptions()
                .Configure(configureOptions);

            //var settings = services.BuildServiceProvider().GetService<IOptions<AzureCosmosDbOptions>>().Value;

            services
                .AddSingleton<MongoDbConnectionProvider>()
                .AddSingleton<IConnectionProvider>(provider => provider.GetRequiredService<MongoDbConnectionProvider>())
                .AddSingleton<IEntityStoreConnectionProvider>(provider => provider.GetRequiredService<MongoDbConnectionProvider>())
                .AddSingleton<IEntityStore>(provider =>
                    {

                        var options = provider.GetRequiredService<IOptions<MongoDbOptions>>().Value;
                        options.CacheClient = provider.GetRequiredService<ICacheClient>();
                        options.LoggerFactory = provider.GetRequiredService<ILoggerFactory>();

                        return new MongoDbEntityStore(options, provider.GetRequiredService<MongoDbConnectionProvider>());

                    }
);

            return services;
        }
    }
}
