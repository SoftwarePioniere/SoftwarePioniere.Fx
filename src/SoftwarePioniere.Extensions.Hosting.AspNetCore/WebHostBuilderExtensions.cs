using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SoftwarePioniere.Extensions.Builder;

namespace SoftwarePioniere.Extensions.Hosting
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseSopi(this IWebHostBuilder webHostBuilder,
            Action<IConfigurationBuilder> configBuilderAction, Action<ISopiBuilder> setupAction,
            Action<IApplicationBuilder> configureApp)
        {
            webHostBuilder.ConfigureAppConfiguration(configBuilderAction)
                .UseSerilog()
                .UseKestrel(k => k.AddServerHeader = false)
                .ConfigureServices((context, services) =>
                {
                    var sopiBuilder = services.AddSopi(opt => context.Configuration.Bind(opt));
                    setupAction(sopiBuilder);

                    services.AddHostedService<SopiAppService>();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<SerilogMiddleware>();
                    configureApp(app);
                });

            return webHostBuilder;
        }
    }
}