using ConsoleApp.Sagas;
using Lib.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Hosting;

namespace ConsoleApp
{
    class Program
    {
        public static int Main() => StartApp();

        private static int StartApp()
        {
       
            AppConfig.SetEnvironmentVariables("SopiSample Console");
            
            return SopiConsoleHost.Run(AppConfig.Configure,
                sopiBuilder =>
                {
                    sopiBuilder.AddDomainServices();
                    sopiBuilder.Services.AddSingleton<ISaga, MySaga>();
                });

        }
    }
}
