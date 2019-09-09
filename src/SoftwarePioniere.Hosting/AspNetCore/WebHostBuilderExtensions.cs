using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
            Action<IConfigurationBuilder> configureConfigurationBuilder,
            Action<ISopiBuilder> configureSopiBuilder,
            Action<IApplicationBuilder> configureApp,
            bool configureAppDefault = true)
        {
            webHostBuilder.ConfigureAppConfiguration(configureConfigurationBuilder)
                .UseSerilog()
                .UseApplicationInsights()
                .UseKestrel(k => k.AddServerHeader = false)
                .ConfigureServices((context, services) =>
                {
                    services.AddScoped<SopiLifetimeActionFilter>();

                    services.Configure<MvcOptions>(options =>
                    {
                        options.Filters.Add(typeof(SopiLifetimeActionFilter));

                    });

                    var sopiBuilder = services.AddSopi(context.Configuration);
                    
                    sopiBuilder
                        .AddPlatformServices()
                        .AddDevOptions()
                        .AddReportingOptions()
                        .AddAppInsightsTelemetry()
                        .AddAuthentication()
                        .AddSystemServicesByConfiguration()
                        .AddMvcServices()
                        .AddClients()
                        ;
                    
                  
                    configureSopiBuilder(sopiBuilder);

                    services.AddHostedService<SopiAppService>();
                })
                .Configure(app =>
                {
                    if (configureAppDefault)
                    {
                        app
                            .UseMiddleware<SerilogMiddleware>()
                            //.UseMiddleware<SopiLifetimeMiddleware>()
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
                      //  app.ApplicationServices.CheckSystemState();
                    }

                });

            return webHostBuilder;
        }
    }
}