using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
using SoftwarePioniere.Extensions.Builder;

namespace SoftwarePioniere.Extensions.Hosting
{
    public static class ApplicationInsightsSerilogExtensions
    {
        public static ILogger CreateSeriloggerWithApplicationInsights(this IConfiguration config,
            Action<LoggerConfiguration> setupAction = null)
        {

            var appInsightsKey = config.GetSection("ApplicationInsights").GetValue<string>("InstrumentationKey");

            return config.CreateSerilogger(logggerConfiguration =>
            {
                setupAction?.Invoke(logggerConfiguration);

                if (!string.IsNullOrEmpty(appInsightsKey))
                {
                    Console.WriteLine("ConfigureLogger:: Adding ApplicationInsightsTraces");
                    logggerConfiguration.WriteTo.ApplicationInsights(appInsightsKey, new TraceTelemetryConverter());
                }

            });
        }
    }
}
