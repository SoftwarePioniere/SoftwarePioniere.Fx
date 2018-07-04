using System;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.ReadModel;
using SoftwarePioniere.ReadModel.Services;

// ReSharper disable once CheckNamespace
namespace SoftwarePioniere.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInMemoryEntityStore(this IServiceCollection services) =>
            services.AddInMemoryEntityStore(_ => { });

        // ReSharper disable once MemberCanBePrivate.Global
        public static IServiceCollection AddInMemoryEntityStore(this IServiceCollection services, Action<InMemoryEntityStoreOptions> configureOptions)
        {

            services
                .AddOptions()
                .Configure(configureOptions);

            services
                .AddSingleton<InMemoryEntityStoreConnectionProvider>()
                .AddSingleton<IEntityStoreConnectionProvider>(provider => provider.GetService<InMemoryEntityStoreConnectionProvider>())
                .AddSingleton<IEntityStore, InMemoryEntityStore>();
            return services;
        }
    }
}
