using SoftwarePioniere.Domain;
using SoftwarePioniere.EventStore.Domain;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventStoreDomainServicesDependencyInjectionExtensions
    {
        public static IServiceCollection AddEventStoreDomainServices(this IServiceCollection services)
        {
            services
                .AddSingleton<DomainEventStore>()
                //   .AddDomainServices()
                .AddSingleton<IEventStore>(p => p.GetRequiredService<DomainEventStore>())
                .AddSingleton<EventStoreReader>()
                .AddSingleton<IEventStoreReader>(p=>p.GetRequiredService<EventStoreReader>())
                ;

            return services;
        }


        public static IServiceCollection AddEventStorePersistentSubscription(this IServiceCollection services)
        {
            services
                .AddSingleton<IPersistentSubscriptionFactory, PersistentSubscriptionFactory>();

            return services;
        }
    }


}
