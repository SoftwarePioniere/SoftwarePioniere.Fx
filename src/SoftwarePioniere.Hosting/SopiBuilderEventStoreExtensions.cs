using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SoftwarePioniere.Builder;
using SoftwarePioniere.EventStore;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderEventStoreExtensions
    {
        public static ISopiBuilder AddEventStore(this ISopiBuilder builder)
        {
            var config = builder.Config;

            Console.WriteLine($"Fliegel 365 DomainEventStore Config Value: {builder.Options.DomainEventStore}");
            switch (builder.Options.DomainEventStore)
            {
                case SopiOptions.DomainEventStoreEventStore:
                    Console.WriteLine("Adding AddDomainEventStore");
                    builder.AddDomainEventStore(c => config.Bind("EventStore", c));
                    break;
            }

            return builder;
        }

        public static ISopiBuilder AddDomainEventStore(this ISopiBuilder builder)
        {
            return builder.AddDomainEventStore(c => { });
        }

        public static ISopiBuilder AddDomainEventStore(this ISopiBuilder builder,
            Action<EventStoreOptions> configureOptions)
        {
            var opt = new EventStoreOptions();
            configureOptions.Invoke(opt);

            builder.Services.AddEventStoreConnection(configureOptions);

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.DomainEventStore = SopiOptions.DomainEventStoreEventStore;
            });

            var options = new EventStoreOptions();
            configureOptions(options);


            builder.GetHealthChecksBuilder()
                .Add(new HealthCheckRegistration(
                    "eventstore",
                    provider => new SopiEventStoreHealthCheck(provider.GetRequiredService<EventStoreConnectionProvider>()),
                    null,
                    null));

            return builder;

        }
    }
}