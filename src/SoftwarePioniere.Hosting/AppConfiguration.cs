using System;
using Microsoft.Extensions.Configuration;

namespace SoftwarePioniere.Hosting
{
    public static class AppConfiguration
    {
        public static IConfiguration CreateConfiguration(Action<IConfigurationBuilder> preSetup = null, Action<IConfigurationBuilder> postSetup = null)
        {
            var builder = new ConfigurationBuilder();

            preSetup?.Invoke(builder);

            postSetup?.Invoke(builder);

            return builder.Build();
        }
        

        public static void ConfigureAppConfig(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables();
        }
    }
}