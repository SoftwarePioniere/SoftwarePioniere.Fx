using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SoftwarePioniere.Builder;

namespace SoftwarePioniere.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseSopi(this IHostBuilder hostBuilder,
            Action<IConfigurationBuilder> configBuilderAction, Action<ISopiBuilder> setupAction)
        {
            hostBuilder.UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddSerilog();
                })
                .ConfigureHostConfiguration(configBuilderAction)
                .ConfigureServices((context, services) =>
                {
                    var sopiBuilder = services.AddSopi(context.Configuration);

                    sopiBuilder
                        .AddPlatformServices()
                        .AddDevOptions()
                        .AddLifetimeOptions()
                        .AddReportingOptions()
                        .AddMessageBusOptions()
                        .AddDefaultTelemetry()
                        .AddSystemServicesByConfiguration()
                        .AddClients()
                        ;

                    setupAction(sopiBuilder);


                    services.AddHostedService<SopiAppService>();
                })
                .UseConsoleLifetime()
                ;

            return hostBuilder;
        }
    }
}