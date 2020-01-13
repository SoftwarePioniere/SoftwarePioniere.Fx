using ConsoleApp.Sagas;
using Lib.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Hosting;

namespace ConsoleApp
{
    class Program
    {
        public static void Main(string[] args) => StartApp(args);

        private static void StartApp(string[] args)
        {

            MyAppConfig.SetEnvironmentVariables("consoleback");

            var config = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(MyAppConfig.Configure)
                .ConfigureServices((context, collection) => collection.AddSingleton(context.Configuration))
                .Build().Services.GetRequiredService<IConfiguration>();

            var serlogger = config.CreateSerilogger();

            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(MyAppConfig.Configure)
                .ConfigureServices((context, services) =>
               {
                   var sopiBuilder = services
                           .AddSopi(context.Configuration, serlogger.Information)
                           .AddPlatformServices()
                           .AddSystemServicesByConfiguration()
                       ;


                   sopiBuilder
                       .AddDomainServices();

                   services
                       .AddSingleton<ISaga, MySaga>()


                       .AddHostedService<SopiAppService>()

                       ;

               })
                .Build().Run()
                ;

        }
    }
}
