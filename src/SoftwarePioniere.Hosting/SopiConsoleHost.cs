//using System;
//using System.IO;
//using System.Reflection;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Serilog;
//using SoftwarePioniere.Builder;

//namespace SoftwarePioniere.Hosting
//{



//    public static class SopiConsoleHost
//    {
//        public static int Run(Action<IConfigurationBuilder> configBuilderAction,
//            Action<ISopiBuilder> setupAction,
//            Action<HostBuilderContext, IServiceCollection> configureServices = null)
//        {
//            var hostBuilder = new HostBuilder();

//            var config = AppConfiguration.CreateConfiguration(b =>
//            {
//                var ass = Assembly.GetEntryAssembly();
//                if (ass != null)
//                {
//                    var path = Path.GetDirectoryName(ass.Location);
//                    Console.WriteLine("Setting config base Path {0}", path);
//                    b.SetBasePath(path);

//                }
//            }, configBuilderAction);

//            var logger = config
//                .CreateSerilogger()
//                .ForContext(typeof(SopiConsoleHost));


//            try
//            {
//                logger.Debug("Starting Building Host");

//                hostBuilder.UseSopi(configBuilderAction, setupAction);

//                if (configureServices != null)
//                    hostBuilder.ConfigureServices(configureServices);

//                var host = hostBuilder
//                    .Build();

//                logger.Information("Running Host");
//                host.Run();
//                logger.Information("Host stopped");
//                return 0;
//            }
//            catch (Exception ex)
//            {
//                logger.Fatal(ex, "Host terminated unexpectedly.");
//                return -1;
//            }
//            finally
//            {
//                Log.CloseAndFlush();
//            }
//        }
//    }
//}