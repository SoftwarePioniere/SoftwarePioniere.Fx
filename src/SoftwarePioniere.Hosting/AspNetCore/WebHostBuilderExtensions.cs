using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SoftwarePioniere.AspNetCore.Builder;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;

namespace SoftwarePioniere.Hosting.AspNetCore
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseSopi(this IWebHostBuilder webHostBuilder,
            Action<IConfigurationBuilder> configBuilderAction, Action<ISopiBuilder> setupAction,
            Action<IApplicationBuilder> configureApp, bool configureAppDefault = true)
        {
            webHostBuilder.ConfigureAppConfiguration(configBuilderAction)
                .UseSerilog()
                .UseApplicationInsights()
                .UseKestrel(k => k.AddServerHeader = false)
                .ConfigureServices((context, services) =>
                {
                    var sopiBuilder = services.AddSopi(context.Configuration);

                    sopiBuilder
                        .AddPlatformServices()
                        .AddDevOptions()
                        .AddReportingOptions()
                        .AddAppInsightsTelemetry()
                        .AddMvcServices()
                        .AddAuthentication()
                        .AddSystemServicesByConfiguration()
                        .AddClients()
                        ;

                    setupAction(sopiBuilder);

                    services.AddHostedService<SopiAppService>();
                })
                .Configure(app =>
                {
                    if (configureAppDefault)
                    {
                        app
                            .UseMiddleware<SerilogMiddleware>()
                            .UseSopiHealthChecks()
                            .UseCors()
                            .UseDefaultFiles()
                            .UseStaticFiles()
                            .UseAuthentication()
                            .UseSopiSwagger()
                            ;
                    }
                    configureApp(app);

                    if (configureAppDefault)
                    {
                        app.UseMvc();
                        app.ApplicationServices.CheckSystemState();
                    }
                  
                });

            return webHostBuilder;
        }
    }
}