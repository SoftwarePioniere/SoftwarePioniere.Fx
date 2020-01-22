using System;
using Lib.Hosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SoftwarePioniere;
using SoftwarePioniere.AspNetCore;
using SoftwarePioniere.AspNetCore.Builder;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.Hosting.AspNetCore;
using SoftwarePioniere.Projections;
using WebApp.Clients;
using WebApp.Controller;

namespace WebApp.Host
{
    public static class Program
    {
        private static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            var id = Environment.GetEnvironmentVariable("APPID");

            if (string.IsNullOrEmpty(id))
            {
                id = "app";
            }

            MyAppConfig.SetEnvironmentVariables(id);
            MyAppConfig.SetWebSocketEnvironmentVariables(Constants.NotificationsBaseRouteAuth);

            var config = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(MyAppConfig.Configure)
                .ConfigureServices((context, collection) => collection.AddSingleton(context.Configuration))
                .Configure(builder => { })
                .Build().Services.GetRequiredService<IConfiguration>();


            var serlogger = config.CreateSerilogger();

            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(MyAppConfig.Configure)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices((context, services) =>
                    {
                        var sopiBuilder = services
                                .AddSopi(context.Configuration, serlogger.Information)
                                .AddAuthentication()
                                .AddPlatformServices()
                                .AddMvcServices(mvc =>
                                {
                                    mvc.AddNewtonsoftJson();
                                })
                                .AddSystemServicesByConfiguration()
                            ;

                        var startDomainServices = Environment.GetEnvironmentVariable("DOMAIN_SERVICES");

                        if (string.IsNullOrEmpty(startDomainServices) || startDomainServices.ToLower() == "true")
                        {
                            sopiBuilder
                                .AddDomainServices();

                            services
                                .AddSingleton<ISaga, MySaga1>();
                        }

                        var startProjectionServices = Environment.GetEnvironmentVariable("PROJECTION_SERVICES");

                        if (string.IsNullOrEmpty(startProjectionServices) || startProjectionServices.ToLower() == "true")
                        {
                            sopiBuilder
                                .AddProjectionServices();

                            services
                                .AddSingleton<MyProjector1>()
                                .AddSingleton<IReadModelProjector, MyCompositeProjector1>();
                        }

                        services.AddSopiSwaggerForMultipleServices(Constants.ApiTitle,
                                 Constants.ApiBaseRoute,
                                 "sample",
                                 new[]
                                 {
                                     Constants.ApiKey, "api", "api2", "cmd1", "qry1"
                                 },
                                 false, options =>
                                 {
                                     options.SchemaFilter<RemoveAllOffSchemaFilter>();
                                 }, sopiBuilder.Log);

                        services
                            .AddTestClientOptions(c => { c.BaseAddress = "http://localhost:5099"; })
                            .AddTestSystemClient()
                            .AddTestUserClient()
                            .AddSingleton<IReadModelProjector, MyProjector1>()
                            .AddSopiService<DelayStartService>()
                            .AddHostedService<SopiAppService>()
                            ;



                        services.AddSignalR(options => { options.EnableDetailedErrors = true; })
                            ;

                        var startQueryServices = Environment.GetEnvironmentVariable("QUERY_SERVICES");

                        if (string.IsNullOrEmpty(startQueryServices) || startQueryServices.ToLower() == "true")
                        {
                            services
                                .AddTransient<MyQueryService>()
                                ;
                        }

                        sopiBuilder.GetMvcBuilder().AddApplicationPart(typeof(HomeController).Assembly);
                    });


                    webBuilder.Configure(app =>
                    {
                        var logger = app.ApplicationServices.GetStartupLogger();
                        app.UseSerilogRequestLogging();

                        app.UseVersionInfo(Constants.ApiBaseRoute, s => logger.LogInformation(s));
                        app.UseSopiLifetimeEndpoint(Constants.ApiBaseRoute, s => logger.LogInformation(s));
                        app.UseProjectionStatusEndpoint(Constants.ProjectionBaseRoute, s => logger.LogInformation(s));

                        app.UseRouting()
                            .UseAuthentication()
                            .UseAuthorization();

                        app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                                endpoints.MapHub<TestHub>(Constants.NotificationsBaseRoute);
                                endpoints.MapHub<TestHubAuth>(Constants.NotificationsBaseRouteAuth);
                            })
                            .UseSopiSwagger();
                    });
                });
        }

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
    }
}