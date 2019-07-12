using System;
using Microsoft.Extensions.Configuration;

namespace SoftwarePioniere.Hosting
{
    public static class AppConfiguration
    {
        public static IConfiguration CreateConfiguration(Action<IConfigurationBuilder> setupAction = null)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables();

            setupAction?.Invoke(builder);

            return builder.Build();
        }
    }
}