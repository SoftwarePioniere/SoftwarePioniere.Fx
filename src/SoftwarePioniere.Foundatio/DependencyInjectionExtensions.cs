using Foundatio.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace SoftwarePioniere
{
    public static class DependencyInjectionExtensions
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
