using SoftwarePioniere.Telemetry;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class TelemetryDependencyInjectionExtensions
    {
    

        public static IServiceCollection AddDefaultTelemetryAdapter(this IServiceCollection services)
        {
            return services.AddSingleton<ITelemetryAdapter, DefaultTelemetryAdapter>();
        }
    }
}
