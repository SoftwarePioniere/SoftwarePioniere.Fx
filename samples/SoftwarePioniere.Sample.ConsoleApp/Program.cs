using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.DomainModel;
using SoftwarePioniere.Extensions.Hosting;
using SoftwarePioniere.Sample.ConsoleApp.Sagas;
using SoftwarePioniere.Sample.Hosting;

namespace SoftwarePioniere.Sample.ConsoleApp
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
