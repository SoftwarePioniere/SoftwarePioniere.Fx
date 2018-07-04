using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.DomainModel;
using SoftwarePioniere.DomainModel.Services;

// ReSharper disable once CheckNamespace
namespace SoftwarePioniere.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            return services.AddTransient<IRepository, Repository>();
        }

        public static IServiceCollection AddInMemoryDomainServices(this IServiceCollection services)
        {
            return services
                    .AddDomainServices()
                    .AddSingleton<IEventStore, InMemoryEventStore>()
                    .AddSingleton<IEventStoreInitializer, EmptyEventStoreInitializer>()
                ;
        }


    }
}
