﻿using System;
using System.Runtime.InteropServices.ComTypes;
using EventStore.ClientAPI;
using SoftwarePioniere.Domain;
using SoftwarePioniere.EventStore;
using SoftwarePioniere.EventStore.Domain;
using SoftwarePioniere.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventStoreDependencyInjectionExtensions
    {

        public static IServiceCollection AddEventStoreConnection(this IServiceCollection services, Action<EventStoreOptions> configureOptions,
            Action<ConnectionSettingsBuilder> connectionSetup = null)
        {

            var opt = services.AddOptions()
                .Configure(configureOptions)
                ;

            if (connectionSetup != null)
            {
                opt.PostConfigure<EventStoreOptions>(options => options.ConnectionSetup = connectionSetup);
            }

            services

                .AddSingleton<EventStoreConnectionProvider>()
                .AddSingleton<IConnectionProvider>(p => p.GetRequiredService<EventStoreConnectionProvider>())
                .AddTransient<EventStoreSetup>()
                .AddTransient<IEventStoreSetup>(p => p.GetRequiredService<EventStoreSetup>())
                //  .AddTransient<IHostedService, EventStoreInitializerBackgroundService>()
                .AddSingleton<IEventStoreInitializer, EventStoreSecurityInitializer>()
                ;

            return services;
        }
    }
}
