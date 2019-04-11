using System;
using Foundatio.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.ReadModel;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReadModelServicesDependencyInjectionExtensions
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
                .AddSingleton<IEntityStore>(provider =>
                {
                    var options = provider.GetRequiredService<IOptions<InMemoryEntityStoreOptions>>().Value;
                    options.CacheClient = provider.GetRequiredService<ICacheClient>();
                    options.LoggerFactory = provider.GetRequiredService<ILoggerFactory>();

                    return new InMemoryEntityStore(
                        options,
                        provider.GetRequiredService<InMemoryEntityStoreConnectionProvider>()
                    );
                });
            return services;
        }
    }
}
