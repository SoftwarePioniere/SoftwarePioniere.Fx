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
             //   .AddDomainServices()
                .AddSingleton<IEventStore, DomainEventStore>()           
                .AddSingleton<IEventStoreReader, EventStoreReader>()
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
