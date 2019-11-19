using Lib.Hosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        public static int Main(string[] args) => StartWebServer(args);

        private static int StartWebServer(string[] args)
        {
            AppConfig.SetEnvironmentVariables("SopiSample WebApp");
            AppConfig.SetWebSocketEnvironmentVariables(Constants.NotificationsBaseRouteAuth);


            return SopiWebHost.Run(WebHost.CreateDefaultBuilder(args),
                AppConfig.Configure,
                sopiBuilder =>
                {

                    sopiBuilder
                     .AddDomainServices()
                     .AddProjectionServices()
                        ;

                    var services = sopiBuilder.Services;

                    services.AddSopiSwaggerForMultipleServices(Constants.ApiTitle,
                        Constants.ApiBaseRoute,
                        "sample",
                        new[]
                        {
                            Constants.ApiKey, "api", "api2", "cmd1", "qry1"
                        },
                        true);

                    services
                        .AddTestClientOptions(c => { c.BaseAddress = "http://localhost:5099"; })
                        .AddTestSystemClient()
                        .AddTestUserClient()
                        .AddTransient<MyQueryService>()
                        .AddSingleton<ISaga, MySaga1>()

                        //.AddSingleton<IReadModelProjector, MyProjector1>()
                        .AddSingleton<MyProjector1>()
                        .AddSingleton<IReadModelProjector, MyCompositeProjector1>()

                        .AddSopiService<DelayStartService>()


                        .AddSignalR(options => { options.EnableDetailedErrors = true; })
                        ;


                    sopiBuilder.GetMvcBuilder().AddApplicationPart(typeof(HomeController).Assembly);

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

                    app.UseSignalR(routes =>
                    {
                        routes.MapHub<TestHub>(Constants.NotificationsBaseRoute);
                        routes.MapHub<TestHubAuth>(Constants.NotificationsBaseRouteAuth);
                    });
                },
                (context, services) =>
                {

                }
            );

        }
    }
}
