using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Redis;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderCacheExtensions
    {
        public static ISopiBuilder AddCacheClient(this ISopiBuilder builder)
        {
            var config = builder.Config;

            Console.WriteLine($"Sopi CacheClient Config Value: {builder.Options.CacheClient}");
            switch (builder.Options.CacheClient)
            {
                case SopiOptions.CacheRedis:
                    Console.WriteLine("Adding Redis CacheClient");
                    builder.AddRedisCacheClient(c => config.Bind("Redis", c));
                    break;

                default:
                    Console.WriteLine("Adding InMemory CacheClient");
                    builder.AddInMemoryCacheClient();
                    break;
            }

            return builder;
        }

        public static ISopiBuilder AddRedisCacheClient(this ISopiBuilder builder)
        {
            return builder.AddRedisCacheClient(_ => { });
        }

        public static ISopiBuilder AddRedisCacheClient(this ISopiBuilder builder,
            Action<RedisOptions> configureOptions)
        {
            var services = builder.Services;
            services.AddRedisCacheClient(configureOptions);

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
