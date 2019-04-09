using System;
using SoftwarePioniere.DomainModel;
using SoftwarePioniere.DomainModel.Services;
using SoftwarePioniere.Messaging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services, Action<RepositoryOptions> configureRepository = null)
        {
            if (configureRepository != null)
            {
                services.AddOptions()
                    .Configure(configureRepository);

            }
            else
            {
                services
                    .AddOptions<RepositoryOptions>()
                    ;
            }

            return services
                    .AddTransient<IRepository, Repository>()
                   
                ;
        }

        public static IServiceCollection AddDefaultMessageBusAdapter(this IServiceCollection services)
        {
            return services.AddSingleton<IMessageBusAdapter, DefaultMessageBusAdapter>();
        }

        public static IServiceCollection AddInMemoryDomainServices(this IServiceCollection services)
        {
            return services
                    //     .AddDomainServices()

                    .AddSingleton<IEventStore, InMemoryEventStore>()
                    .AddSingleton<IProjectionReader, NullProjectionReader>()
                //    .AddSingleton<IEventStoreInitializer, EmptyEventStoreInitializer>()
                ;
        }




    }
}
