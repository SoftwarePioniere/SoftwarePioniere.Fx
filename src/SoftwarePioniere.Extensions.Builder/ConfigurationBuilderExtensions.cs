using Microsoft.Extensions.Configuration;

namespace SoftwarePioniere.Extensions.Builder
{
    public static class ConfigurationBuilderExtensions
    {

        //public static Lazy<IConfiguration> Configuration => new Lazy<IConfiguration>(CreateConfigurationBuilder().Build, LazyThreadSafetyMode.ExecutionAndPublication);

        //private static IConfigurationBuilder CreateConfigurationBuilder()
        //{
        //    var builder = new ConfigurationBuilder()
        //        .AddJsonFile("appsettings.json", true)

        //        .AddEnvironmentVariables();

        //    return builder;
        //}

        public static IConfigurationBuilder AddSecretsAndEnvironment(this IConfigurationBuilder configurationBuilder, string prefix)
        {
            configurationBuilder
                //.AddEnvironmentVariables()
                .AddUserSecrets(prefix)
                .AddEnvironmentVariables($"{prefix.ToUpper()}_");

            return configurationBuilder;
        }
    }
}
