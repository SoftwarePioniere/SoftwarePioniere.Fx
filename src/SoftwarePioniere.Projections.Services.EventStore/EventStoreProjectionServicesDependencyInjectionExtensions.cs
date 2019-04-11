using SoftwarePioniere.EventStore;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Projections;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventStoreProjectionServicesDependencyInjectionExtensions
    {
        public static IServiceCollection AddEventStoreProjectionServices(this IServiceCollection services)
        {
            return services
                    .AddSingleton<IEventStoreInitializer, EventStoreSecurityInitializer>()
                    .AddSingleton<IEventStoreInitializer, EventStoreProjectionByCategoryInitializer>()
                     //.AddTransient<IHostedService, ProjectionBackgroundService>()
                    .AddTransient<IProjectorRegistry, EventStoreProjectorRegistry>();

        }

    }
}
