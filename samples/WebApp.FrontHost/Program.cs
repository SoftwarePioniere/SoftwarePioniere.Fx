using Lib.Hosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SoftwarePioniere.AspNetCore;
using SoftwarePioniere.AspNetCore.Builder;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.Hosting.AspNetCore;
using WebApp.Clients;
using WebApp.Controller;

namespace WebApp.FrontHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateWebHostBuilder(string[] args)
        {

            MyAppConfig.SetEnvironmentVariables("fronthost");
            MyAppConfig.SetWebSocketEnvironmentVariables(Constants.NotificationsBaseRouteAuth);

            var config = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(MyAppConfig.Configure)
                .ConfigureServices((context, collection) => collection.AddSingleton(context.Configuration))
                .Configure(builder => { })
                .Build().Services.GetRequiredService<IConfiguration>();

            var serlogger = config.CreateSerilogger();

            return Host.CreateDefaultBuilder(args)
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

                        services.AddSopiSwaggerForMultipleServices(Constants.ApiTitle,
                            Constants.ApiBaseRoute,
                            "sample",
                            new[]
                            {
                                Constants.ApiKey, "api", "api2", "cmd1", "qry1"
                            },
                            false);


                        services
                            .AddTestClientOptions(c => { c.BaseAddress = "http://localhost:5097"; })
                            .AddTestSystemClient()
                            .AddTestUserClient()

                            .AddSignalR(options => { options.EnableDetailedErrors = true; })
                            ;


                        services
                            .AddTransient<MyQueryService>()
                            ;


                        sopiBuilder.GetMvcBuilder().AddApplicationPart(typeof(HomeController).Assembly);


                    });

                    webBuilder.Configure(app =>
                    {

                        var logger = app.ApplicationServices.GetStartupLogger();
                        app.UseSerilogRequestLogging();

                        app.UseVersionInfo(Constants.ApiBaseRoute);
                        app.UseSopiLifetimeEndpoint(Constants.ApiBaseRoute);
                   
                        logger.LogInformation("WebSocket Url: {WebSocketUrl}", Constants.NotificationsBaseRoute);
                        logger.LogInformation("WebSocket AuthUrl: {WebSocketUrl}", Constants.NotificationsBaseRouteAuth);

                        
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


                })
                ;

        }
    }
}
