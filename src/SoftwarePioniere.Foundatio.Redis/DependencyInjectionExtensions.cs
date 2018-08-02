using System;
using Foundatio.Caching;
using Foundatio.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Foundatio.Redis;
using StackExchange.Redis;

// ReSharper disable once CheckNamespace
namespace SoftwarePioniere.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {

        public static IServiceCollection AddRedisMessageBus(this IServiceCollection services, string topic)
        {
            return services.AddRedisMessageBus(topic, _ => { });
        }

        public static IServiceCollection AddRedisMessageBus(this IServiceCollection services, string topic, Action<RedisOptions> configureOptions)
        {
            services
                .AddOptions()
                .Configure(configureOptions);

            services
               .AddSingleton(p => ConnectionMultiplexer.Connect(p.GetRequiredService<IOptions<RedisOptions>>().Value.ConnectionString))
               .AddSingleton<IConnectionMultiplexer>(c => c.GetRequiredService<ConnectionMultiplexer>())
               ;

            services.AddSingleton<IMessageBus>(p =>
                new RedisMessageBus(o =>
                    o.LoggerFactory(p.GetRequiredService<ILoggerFactory>())
                        .Subscriber(p.GetRequiredService<IConnectionMultiplexer>().GetSubscriber())                        
                        .Topic(topic)));

            services.AddSingleton<IMessageSubscriber>(c => c.GetRequiredService<IMessageBus>())
                .AddSingleton<IMessagePublisher>(c => c.GetRequiredService<IMessageBus>());

            return services;
        }


        public static IServiceCollection AddRedisCacheClient(this IServiceCollection services)
        {
            return services.AddRedisCacheClient(_ => { });
        }

        public static IServiceCollection AddRedisCacheClient(this IServiceCollection services,
            Action<RedisOptions> configureOptions)
        {
            services
                .AddOptions()
                .Configure(configureOptions);

            services
               .AddSingleton(p => ConnectionMultiplexer.Connect(p.GetRequiredService<IOptions<RedisOptions>>().Value.ConnectionString))
               .AddSingleton<IConnectionMultiplexer>(c => c.GetRequiredService<ConnectionMultiplexer>())
               ;

            services.AddSingleton<ICacheClient>(p =>
                new RedisCacheClient(o =>
                    o.LoggerFactory(p.GetRequiredService<ILoggerFactory>())
                        .ConnectionMultiplexer(p.GetRequiredService<ConnectionMultiplexer>())
                ));

            return services;
        }
    }
}
