using System;
using Lib.Hosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.AspNetCore;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.Hosting.AspNetCore;
using SoftwarePioniere.Projections;

namespace WebApp.BackHost
{
    public static class Program
    {
        public static int Main(string[] args) => StartWebServer(args);

        private static int StartWebServer(string[] args)
        {
            //var id = Environment.GetEnvironmentVariable("APPID");

            //if (string.IsNullOrEmpty(id))
            var id = "backhost";

            AppConfig.SetEnvironmentVariables(id);

            //AppConfig.SetWebSocketEnvironmentVariables(Constants.NotificationsBaseRouteAuth);


            return SopiWebHost.Run(WebHost.CreateDefaultBuilder(args),
                AppConfig.Configure,
                sopiBuilder =>
                {
                    var services = sopiBuilder.Services;
                    //var startDomainServices = Environment.GetEnvironmentVariable("DOMAIN_SERVICES");

                    //if (string.IsNullOrEmpty(startDomainServices) || startDomainServices.ToLower() == "true")
                    //{
                    sopiBuilder
                        .AddDomainServices();

                    services
                        .AddSingleton<ISaga, MySaga1>();

                    //}

                    //var startProjectionServices = Environment.GetEnvironmentVariable("PROJECTION_SERVICES");

                    //if (string.IsNullOrEmpty(startProjectionServices) || startProjectionServices.ToLower() == "true")
                    //{
                        sopiBuilder
                            .AddProjectionServices();

                        services
                            .AddSingleton<MyProjector1>()
                            .AddSingleton<IReadModelProjector, MyCompositeProjector1>();

                    //}

                    services.AddSopiSwaggerForMultipleServices(Constants.ApiTitle,
                             Constants.ApiBaseRoute,
                             "backhost",
                             new[]
                             {
                            Constants.ApiKey, "api", "api2", "cmd1", "qry1"
                             },
                             false);

                    //services
                    //.AddTestClientOptions(c => { c.BaseAddress = "http://localhost:5099"; })
                    //.AddTestSystemClient()
                    //.AddTestUserClient()
                    //.AddSingleton<IReadModelProjector, MyProjector1>()
                    //.AddSopiService<DelayStartService>()
                    //.AddSignalR(options => { options.EnableDetailedErrors = true; })
                    //;


                    //var startQueryServices = Environment.GetEnvironmentVariable("QUERY_SERVICES");

                    //if (string.IsNullOrEmpty(startQueryServices) || startQueryServices.ToLower() == "true")
                    //{
                    //    services
                    //        .AddTransient<MyQueryService>()
                    //        ;


                    //}

                    //sopiBuilder.GetMvcBuilder().AddApplicationPart(typeof(HomeController).Assembly);

                },
                app =>
                {
                    var logger = app.ApplicationServices.GetStartupLogger();

                    app.UseVersionInfo(Constants.ApiBaseRoute);
                    app.UseSopiLifetimeEndpoint(Constants.ApiBaseRoute);
                    app.UseProjectionStatusEndpoint(Constants.ProjectionBaseRoute);

                    //   app.UseSopiLifetime();
                    logger.LogInformation("WebSocket Url: {WebSocketUrl}", Constants.NotificationsBaseRoute);
                    logger.LogInformation("WebSocket AuthUrl: {WebSocketUrl}", Constants.NotificationsBaseRouteAuth);

                    //app.UseSignalR(routes =>
                    //{
                    //    routes.MapHub<TestHub>(Constants.NotificationsBaseRoute);
                    //    routes.MapHub<TestHubAuth>(Constants.NotificationsBaseRouteAuth);
                    //});
                },
                (context, services) =>
                {

                }
            );

        }
    }
}
