using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.AspNetCore;
using SoftwarePioniere.Extensions.Builder;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Extensions.Hosting
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
