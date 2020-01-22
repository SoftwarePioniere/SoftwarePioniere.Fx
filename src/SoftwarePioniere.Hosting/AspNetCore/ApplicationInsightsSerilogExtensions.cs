using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

namespace SoftwarePioniere.Hosting.AspNetCore
{
    public static class ApplicationInsightsSerilogExtensions
    {
        public static ILogger CreateSeriloggerWithApplicationInsights(this IConfiguration config, Action<string> log,
            Action<LoggerConfiguration> setupAction = null)
        {

            var appInsightsKey = config.GetSection("ApplicationInsights").GetValue<string>("InstrumentationKey");

            return config.CreateSerilogger(
                logggerConfiguration =>
            {
                setupAction?.Invoke(logggerConfiguration);

                if (!string.IsNullOrEmpty(appInsightsKey))
                {
                    log("ConfigureLogger:: Adding ApplicationInsightsTraces");
                    logggerConfiguration.WriteTo.ApplicationInsights(appInsightsKey, new TraceTelemetryConverter());
                }

            });
        }
    }
}
