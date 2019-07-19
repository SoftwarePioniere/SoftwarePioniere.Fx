using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Redis;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderMessageBusExtensions
    {
           public static ISopiBuilder AddMessageBus(this ISopiBuilder builder)
        {
            var config = builder.Config;

            builder.Services.AddCachingOptions();
                
            
            Console.WriteLine($"Fliegel 365 MessageBus Config Value: {builder.Options.MessageBus}");
            switch (builder.Options.MessageBus)
            {
                case SopiOptions.MessageBusRedis:
                    Console.WriteLine("Adding Redis Message Bus");
                    builder.AddRedisMessageBus(c => config.Bind("Redis", c));
                    break;

                //case SopiOptions.MessageBusRabbitMq:
                //    Console.WriteLine("Adding RabbitMQ Message Bus");
                //    builder.AddRabbitMqMessageBus(c => config.Bind("RabbitMQ", c));
                //    break;

                default:
                    Console.WriteLine("Adding InMemory Bus");
                    builder.AddInMemoryMessageBus();
                    break;
            }

            return builder;
        }

        public static ISopiBuilder AddInMemoryMessageBus(this ISopiBuilder builder)
        {
            

            builder.Services.AddInMemoryMessageBus();

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.MessageBus = SopiOptions.InMemory;
            });


            return builder;
        }

        public static ISopiBuilder AddRedisMessageBus(this ISopiBuilder builder)
        {
            return builder.AddRedisMessageBus(_ => { });
        }

        public static ISopiBuilder AddRedisMessageBus(this ISopiBuilder builder,
            Action<RedisOptions> configureOptions)
        {
            var services = builder.Services;
            services.AddRedisMessageBus("Fliegel365", configureOptions);

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.MessageBus = SopiOptions.MessageBusRedis;
            });

            var options = new RedisOptions();
            configureOptions(options);
            builder.GetHealthChecksBuilder()
                .AddRedis(options.ConnectionString, "redis-bus");

            return builder;
        }
    }
}
