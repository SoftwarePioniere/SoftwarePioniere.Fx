using Foundatio.Caching;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class CacheDependencyInjectionExtensions
    {
        public static IServiceCollection AddInMemoryCacheClient(this IServiceCollection services)
        {
            return services.AddSingleton<ICacheClient>(new InMemoryCacheClient(new InMemoryCacheClientOptions
            {
                LoggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>()
            }));
        }
    }
}
