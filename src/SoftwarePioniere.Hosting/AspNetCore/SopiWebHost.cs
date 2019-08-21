using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SoftwarePioniere.Builder;

namespace SoftwarePioniere.Hosting.AspNetCore
{
    public static class SopiWebHost
    {
        public static int Run(IWebHostBuilder webHostBuilder, Action<IConfigurationBuilder> configBuilderAction,
            Action<ISopiBuilder> configureBuilder,
            Action<IApplicationBuilder> configureApp,
            Action<WebHostBuilderContext, IServiceCollection> configureServices = null,
            bool configureAppDefault = true)
        {
            var config = AppConfiguration.CreateConfiguration(configBuilderAction);

            var logger = config.CreateSeriloggerWithApplicationInsights().ForContext(typeof(SopiWebHost));

            try
            {
                logger.Debug("Starting Building Host");

                //var builder = WebHost.CreateDefaultBuilder(args)
                webHostBuilder.UseSopi(configBuilderAction, configureBuilder, configureApp, configureAppDefault);

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