using System;
using Foundatio.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Caching;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class CacheDependencyInjectionExtensions
    {
        public static IServiceCollection AddCachingOptions(this IServiceCollection services, Action<CacheOptions> configureCaching = null)
        {
            if (configureCaching != null)
            {
                services.AddOptions()
                    .Configure(configureCaching);

            }
            else
            {
                services.AddOptions<CacheOptions>();
            }

            return services;
        }

        public static IServiceCollection AddCachingOptions(this IServiceCollection services, IConfiguration config)
        {
            return services.AddCachingOptions(c => config.Bind("Caching", c));
        }

        public static IServiceCollection AddInMemoryCacheClient(this IServiceCollection services)
        {

            //services.AddCachingOptions();

            return services.AddSingleton<ICacheClient>(new InMemoryCacheClient(new InMemoryCacheClientOptions
            {
                LoggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>(),
                MaxItems = int.MaxValue
            }));
        }
    }
}
