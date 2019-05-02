//using SoftwarePioniere.Messaging;

// ReSharper disable once CheckNamespace

using SoftwarePioniere.Messaging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MessagingServicesDependencyInjectionExtensions
    {
        public static IServiceCollection AddDefaultMessageBusAdapter(this IServiceCollection services)
        {
            return services.AddSingleton<IMessageBusAdapter, DefaultMessageBusAdapter>();
        }

        public static IServiceCollection AddDefaultTelemetryAdapter(this IServiceCollection services)
        {
            return services.AddSingleton<ITelemetryAdapter, DefaultTelemetryAdapter>();
        }
    }
}
