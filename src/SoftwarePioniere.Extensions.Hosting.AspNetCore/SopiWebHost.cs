using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SoftwarePioniere.Extensions.Builder;

namespace SoftwarePioniere.Extensions.Hosting
{
    public static class SopiWebHost
    {
        public static int Run(IWebHostBuilder webHostBuilder, Action<IConfigurationBuilder> configBuilderAction,
            Action<ISopiBuilder> setupAction,
            Action<IApplicationBuilder> configureApp,
            Action<WebHostBuilderContext, IServiceCollection> configureServices = null)
        {
            var config = AppConfiguration.CreateConfiguration(configBuilderAction);

            var logger = config.CreateSeriloggerWithApplicationInsights().ForContext(typeof(SopiWebHost));

            try
            {
                logger.Debug("Starting Building Host");

                //var builder = WebHost.CreateDefaultBuilder(args)
                webHostBuilder.UseSopi(configBuilderAction, setupAction, configureApp);

                if (configureServices != null)
                {
                    webHostBuilder.ConfigureServices(configureServices);
                }

                var host = webHostBuilder
                    .Build();

                logger.Information("Running Host");
                host.Run();
                logger.Information("Host stopped");
                return 0;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Host terminated unexpectedly.");
                return -1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

    }
}