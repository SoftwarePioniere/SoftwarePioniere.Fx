using System;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.AspNetCore;
using SoftwarePioniere.DomainModel;
using SoftwarePioniere.Extensions.Hosting;
using SoftwarePioniere.Projections;
using SoftwarePioniere.Sample.Hosting;

namespace SoftwarePioniere.Sample.WebApp.Host
{
    public static class Program
    {
        public static int Main(string[] args) => StartWebServer(args);

        private static int StartWebServer(string[] args)
        {
            AppConfig.SetEnvironmentVariables("SopiSample WebApp");
            AppConfig.SetWebSocketEnvironmentVariables(Constants.NotificationsBaseRouteAuth);


            return SopiWebHost.Run(WebHost.CreateDefaultBuilder(args), AppConfig.Configure,
                sopiBuilder =>
                {

                    sopiBuilder
                        .AddSwaggerForMultipleServices(Constants.ApiTitle,
                            Constants.ApiBaseRoute,
                            "sample",
                            new[]
                            {
                                Constants.ApiKey, "api", "api2", "cmd1", "qry1"
                            })
                        .AddDomainServices()
                        .AddProjectionServices()
                        ;

                    sopiBuilder.Services
                        .AddTestClientOptions(c => { c.BaseAddress = "http://localhost:5099"; })
                        .AddTestSystemClient()
                        .AddTestUserClient()
                        .AddTransient<MyQueryService>()
                        .AddSingleton<ISaga, MySaga1>()

                        //.AddSingleton<IReadModelProjector, MyProjector1>()
                        .AddSingleton<MyProjector1>()
                        .AddSingleton<IReadModelProjector, MyCompositeProjector1>()

                        .AddSignalR(options => { options.EnableDetailedErrors = true; })
                        ;

                    sopiBuilder.GetMvcBuilder().AddApplicationPart(typeof(HomeController).Assembly);

                },
                app =>
                {
                    app.UseVersionInfo(Constants.ApiBaseRoute);
                    app.UseProjectionStatusEndpoint(Constants.ProjectionBaseRoute);

                    Console.WriteLine("WebSocket Url: {0}", Constants.NotificationsBaseRoute);
                    Console.WriteLine("WebSocket Auth Url: {0}", Constants.NotificationsBaseRouteAuth);

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
