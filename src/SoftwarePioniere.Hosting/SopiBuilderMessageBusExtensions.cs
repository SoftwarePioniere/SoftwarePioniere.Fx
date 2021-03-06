﻿using System;
using Foundatio.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Hosting.AzureServiceBus.Queues;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderMessageBusExtensions
    {
        public static ISopiBuilder AddMessageBus(this ISopiBuilder builder)
        {
            var config = builder.Config;

            builder.Services.AddCachingOptions();

            builder.Log($"MessageBus Config Value: {builder.Options.MessageBus}");
            switch (builder.Options.MessageBus)
            {
                case SopiOptions.MessageBusRedis:
                    builder.Log("Adding Redis Message Bus");
                    builder.AddRedisMessageBus();
                    break;

                case SopiOptions.MessageBusAzureServiceBus:
                    builder.Log("Adding AzureServiceBus Message Bus");
                    builder.AddAzureMessageBus(c => config.Bind("AzureServiceBus", c));
                    break;

                //case SopiOptions.MessageBusRabbitMq:
                //    builder.Log("Adding RabbitMQ Message Bus");
                //    builder.AddRabbitMqMessageBus(c => config.Bind("RabbitMQ", c));
                //    break;

                default:
                    builder.Log("Adding InMemory Bus");
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
            var services = builder.Services;
            services.AddRedisMessageBus("Fliegel365");

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.MessageBus = SopiOptions.MessageBusRedis;
            });

            //var options = new RedisOptions();
            //configureOptions(options);
            //builder.GetHealthChecksBuilder()
            //    .AddRedis(options.ConnectionString, "redis-bus");

            return builder;
        }

        public static ISopiBuilder AddAzureMessageBus(this ISopiBuilder builder, Action<AzureServiceBusMessageBusOptions> configureOptions)
        {
            var services = builder.Services;

            services
                .AddOptions()
                .Configure(configureOptions);

            services.AddSingleton<IMessageBus>(p =>
            {
                var opt = p.GetRequiredService<IOptions<AzureServiceBusMessageBusOptions>>();

                var topic = opt.Value.Topic;
                if (string.IsNullOrEmpty(topic))
                    topic = builder.Options.AppId;

                var subscrName = opt.Value.SubscriptionName;
                if (string.IsNullOrEmpty(subscrName))
                    subscrName = builder.Options.AppId;

                var ttl = opt.Value.SubscriptionDefaultMessageTimeToLive.GetValueOrDefault(TimeSpan.FromHours(4));

                var b = new AzureServiceBusMessageBusOptionsBuilder();
                b.ConnectionString(opt.Value.ConnectionString)
                    .LoggerFactory(p.GetRequiredService<ILoggerFactory>())
                    .Topic(topic)
                    .SubscriptionName(subscrName)
                    .SubscriptionDefaultMessageTimeToLive(ttl)
                    ;

                return new AzureServiceBusMessageBus(b.Build());
            });

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.MessageBus = SopiOptions.MessageBusAzureServiceBus;
            });

            services
                .AddSingleton<IMessageSubscriber>(c => c.GetRequiredService<IMessageBus>())
                .AddSingleton<IMessagePublisher>(c => c.GetRequiredService<IMessageBus>())
                .AddSingleton<IQueueFactory, AzureServiceBusQueueFactory>()
                ;


            return builder;
        }
    }
}
