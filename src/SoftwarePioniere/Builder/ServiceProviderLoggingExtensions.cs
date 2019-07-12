using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.Builder
{
    public static class ServiceProviderLoggingExtensions
    {
        public static ILogger GetStartupLogger(this IServiceProvider provider)
        {
            return provider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        }
    }
}
