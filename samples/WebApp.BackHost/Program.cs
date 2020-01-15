using Lib.Hosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SoftwarePioniere.AspNetCore;
using SoftwarePioniere.AspNetCore.Builder;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.Hosting.AspNetCore;
using SoftwarePioniere.Projections;

namespace WebApp.BackHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateWebHostBuilder(string[] args)
        {

            var id = "backhost";
            MyAppConfig.SetEnvironmentVariables(id);

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

                            sopiBuilder
                                .AddDomainServices();

                            services
                                .AddSingleton<ISaga, MySaga1>();


                            sopiBuilder
                                .AddProjectionServices();


                            services
                                .AddSingleton<MyProjector1>()
                                .AddSingleton<IReadModelProjector, MyCompositeProjector1>()
                                
                                .AddHostedService<SopiAppService>()
                                ;


                            services.AddSopiSwaggerForMultipleServices(Constants.ApiTitle,
                                Constants.ApiBaseRoute,
                                "backhost",
                                new[]
                                {
                                    Constants.ApiKey, "api", "api2", "cmd1", "qry1"
                                },
                                false);


                        });

                        webBuilder.Configure(app =>
                        {
                            app.UseSerilogRequestLogging();

                            app.UseVersionInfo(Constants.ApiBaseRoute);
                            app.UseSopiLifetimeEndpoint(Constants.ApiBaseRoute);
                            app.UseProjectionStatusEndpoint(Constants.ProjectionBaseRoute);

                            
                            app.UseRouting()
                                .UseAuthentication()
                                .UseAuthorization();

                        });

                    })
                ;



        }
    }
}
