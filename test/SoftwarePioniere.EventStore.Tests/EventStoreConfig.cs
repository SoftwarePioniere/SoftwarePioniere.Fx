using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.EventStore.Tests
{
    public static class EventStoreConfig
    {
        public static IServiceCollection AddEventStoreTestConfig(this IServiceCollection services, ILogger logger)
        {

            services
                .AddEventStoreConnection(c => new TestConfiguration().ConfigurationRoot.Bind("EventStore", c)

                    , builder => builder.UseCustomLogger(new EventStoreLogger(logger))
                );
            return services;

        }
    }
}