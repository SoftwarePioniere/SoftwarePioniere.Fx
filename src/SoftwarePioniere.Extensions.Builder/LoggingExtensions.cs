using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.Extensions.Builder
{
    public static  class LoggingExtensions
    {
        public static ILogger GetStartupLogger(this IServiceProvider provider)
        {
            return provider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        }
    }
}
