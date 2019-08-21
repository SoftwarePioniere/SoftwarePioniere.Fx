using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.AspNetCore;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Hosting.ApplicationInsights;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Telemetry;

namespace SoftwarePioniere.Hosting.AspNetCore
{
    public static class SopiBuilderAppInsightsTelemetryBuilderExtensions
    {
        public static ISopiBuilder AddAppInsightsTelemetry(this ISopiBuilder builder)
        {
            var services = builder.Services;

            services
                .AddSingleton<AppInsightsTelemetryMessageBusAdapter>()
                .AddSingleton<IMessageBusAdapter>(c => c.GetRequiredService<AppInsightsTelemetryMessageBusAdapter>())       
                .AddSingleton<AppInsightsTelemetryAdapter>()
                .AddSingleton<ITelemetryAdapter>(c => c.GetRequiredService<AppInsightsTelemetryAdapter>())
                .AddSingleton<IControllerTelemetryAdapter>(c => c.GetRequiredService<AppInsightsTelemetryAdapter>())
                ;

            return builder;
        }
    }
}
