using Foundatio.Caching;
using Foundatio.Lock;
using Foundatio.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Domain;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderExtensions
    {
        public static ISopiBuilder AddSystemServicesByConfiguration(this ISopiBuilder builder)
        {
            builder
                .AddMessageBus()
                .AddEntityStore()
                .AddCacheClient()
                .AddStorage()
                .AddEventStore()
                ;

            return builder;
        }

        public static ISopiBuilder AddDefaultTelemetry(this ISopiBuilder builder)
        {
            var services = builder.Services;
            services
              //  .AddDefaultTelemetryAdapter()
                .AddDefaultMessageBusAdapter();

            return builder;

        }

        public static ISopiBuilder AddPlatformServices(this ISopiBuilder builder)
        {

            var services = builder.Services;

            services.AddOptions();

            //builder.AddHealthChecks()
            //    ;

            services.AddSingleton<SopiApplicationLifetime>()
                .AddSingleton<ISopiApplicationLifetime>(p => p.GetRequiredService<SopiApplicationLifetime>())
                ;

            services.AddSingleton(
                resolver => resolver.GetRequiredService<IOptions<SopiOptions>>().Value);

            services.AddSingleton<ILockProvider>(pr =>
                new CacheLockProvider(pr.GetRequiredService<ICacheClient>(),
                    pr.GetRequiredService<IMessageBus>(),
                    pr.GetRequiredService<ILoggerFactory>()
                ));

            services
                .AddSingleton<ICacheAdapter, CacheAdapter>()
                .AddSingleton<ISagaServices, SagaServices>()
             //   .AddSingleton<IProjectorServices, ProjectorServices>()
                .AddDefaultMessageBusAdapter()
              
                ;


            return builder;
        }

        public static ISopiBuilder AddDomainServices(this ISopiBuilder builder)
        {
            builder.Services.AddDomainServices(c => builder.Config.Bind("Repository", c));

            if (builder.Options.DomainEventStore == SopiOptions.DomainEventStoreEventStore)
            {
                builder.Services.AddEventStoreDomainServices()
                    .AddEventStorePersistentSubscription();
            }
            else
            {
                builder.Services.AddInMemoryDomainServices()
                    .AddSingleton<IPersistentSubscriptionFactory, NullPersistentSubscriptionFactory>();
            }

            return builder;
        }

        public static ISopiBuilder AddProjectionServices(this ISopiBuilder builder)
        {
            builder.Services.AddProjectionServices(c => builder.Config.Bind("Projection", c));

            if (builder.Options.DomainEventStore == SopiOptions.DomainEventStoreEventStore)
            {
                builder.Services.AddEventStoreProjectionServices();
            }

            return builder;
        }
    }
}
