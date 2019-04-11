using SoftwarePioniere.DomainModel;
using SoftwarePioniere.DomainModel.Subscriptions;

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
                .AddSingleton<IProjectionReader, ProjectionReader>()
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
