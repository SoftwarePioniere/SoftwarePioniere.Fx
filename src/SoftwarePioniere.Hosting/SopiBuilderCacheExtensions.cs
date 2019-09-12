using System;
using Foundatio.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Redis;
using StackExchange.Redis;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderCacheExtensions
    {
        private static string CreateDatabaseId(ISopiBuilder builder)
        {
            var options = new CacheOptions();
            builder.Config.Bind("Caching", options);

            if (!string.IsNullOrEmpty(options.CacheScope))
                return options.CacheScope;

            if (string.IsNullOrEmpty(builder.Options.AppContext))
            {
                return $"{builder.Options.AppId}-{builder.Version}".Replace(".", "-").Replace(" ", "");
            }
            else
            {
                return $"{builder.Options.AppContext}-{builder.Version}".Replace(".", "-").Replace(" ", "");
            }
        }


        public static ISopiBuilder AddCacheClient(this ISopiBuilder builder)
        {
            var config = builder.Config;

            builder.Services.AddCachingOptions(c => config.Bind("Caching"));


            builder.Log($"Sopi CacheClient Config Value: {builder.Options.CacheClient}");
            switch (builder.Options.CacheClient)
            {
                case SopiOptions.CacheRedis:
                    builder.Log("Adding Redis CacheClient");
                    builder.AddRedisCacheClient(c => config.Bind("Redis", c));
                    break;

                default:
                    builder.Log("Adding InMemory CacheClient");
                    builder.AddInMemoryCacheClient();
                    break;
            }

            return builder;
        }
        
        public static ISopiBuilder AddRedisCacheClient(this ISopiBuilder builder,
            Action<RedisOptions> configureOptions)
        {
            var services = builder.Services;
            //services.AddRedisCacheClient(configureOptions);

            services
                .AddOptions()
                .Configure(configureOptions);

            services
                .AddSingleton(p => ConnectionMultiplexer.Connect(p.GetRequiredService<IOptions<RedisOptions>>().Value.ConnectionString))
                .AddSingleton<IConnectionMultiplexer>(c => c.GetRequiredService<ConnectionMultiplexer>())
                .AddSingleton(p => new RedisCacheClient(o =>
                    o.LoggerFactory(p.GetRequiredService<ILoggerFactory>())
                        .ConnectionMultiplexer(p.GetRequiredService<ConnectionMultiplexer>())
                ));

            services.AddSingleton<ICacheClient>(p => new ScopedCacheClient(p.GetRequiredService<RedisCacheClient>(), CreateDatabaseId(builder)));


            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.MessageBus = SopiOptions.CacheRedis;
            });

            var options = new RedisOptions();
            configureOptions(options);
            builder.GetHealthChecksBuilder()
                .AddRedis(options.ConnectionString, "redis-cache");

            return builder;
        }

        public static ISopiBuilder AddInMemoryCacheClient(this ISopiBuilder builder)
        {
            var services = builder.Services;
            services.AddInMemoryCacheClient();

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.CacheClient = SopiOptions.InMemory;
            });

            return builder;
        }
    }
}
