using System;
using Foundatio.Caching;
using Foundatio.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Redis;
using SoftwarePioniere.Redis.Queues;
using StackExchange.Redis;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisDependencyInjectionExtensions
    {
        public static IServiceCollection AddRedisOptions(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddRedisOptions(c => configuration.Bind("Redis", c));
        }

        public static IServiceCollection AddRedisOptions(this IServiceCollection services, Action<RedisOptions> configureOptions)
        {
            return services
                .AddOptions()
                .Configure(configureOptions);
        }

        public static IServiceCollection AddRedis(this IServiceCollection services)
        {

            services
                .AddSingleton<RedisConnectionProvider>()
                .AddSingleton<IConnectionProvider>(p => p.GetRequiredService<RedisConnectionProvider>())

                .AddSingleton(p => p.GetRequiredService<RedisConnectionProvider>().Connection)

                //  .AddSingleton(p => ConnectionMultiplexer.Connect(p.GetRequiredService<IOptions<RedisOptions>>().Value.ConnectionString))

                .AddSingleton<IConnectionMultiplexer>(c => c.GetRequiredService<ConnectionMultiplexer>())

                //.AddSingleton(p => ConnectionMultiplexer.Connect(p.GetRequiredService<IOptions<RedisOptions>>().Value.ConnectionString))
                //.AddSingleton<IConnectionMultiplexer>(c => c.GetRequiredService<ConnectionMultiplexer>())
                ;


            return services;

        }

        public static IServiceCollection AddRedisMessageBus(this IServiceCollection services, string topic)
        {

            services

               .AddSingleton<IQueueFactory, RedisQueueFactory>()
               ;

            services.AddSingleton<IMessageBus>(p =>
                new RedisMessageBus(o =>
                    o.LoggerFactory(p.GetRequiredService<ILoggerFactory>())
                        .Subscriber(p.GetRequiredService<IConnectionMultiplexer>().GetSubscriber())
                        .Topic(topic)
                    ));

            services
                .AddSingleton<IMessageSubscriber>(c => c.GetRequiredService<IMessageBus>())
                .AddSingleton<IMessagePublisher>(c => c.GetRequiredService<IMessageBus>());

            return services;
        }


        public static IServiceCollection AddRedisCacheClient(this IServiceCollection services)
        {
            services.AddSingleton<ICacheClient>(p =>
                new RedisCacheClient(o =>
                    o.LoggerFactory(p.GetRequiredService<ILoggerFactory>())
                        .ConnectionMultiplexer(

                            p.GetRequiredService<RedisConnectionProvider>().Connection

                            //p.GetRequiredService<ConnectionMultiplexer>()

                            )
                ));

            return services;
        }
    }
}
