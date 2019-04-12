using Microsoft.Extensions.Configuration;

namespace SoftwarePioniere.Extensions.Builder
{
    public static class ConfigurationExtensions
    {
        public static SopiOptions CreateSopiOptions(this IConfiguration config)
        {
            var options = new SopiOptions();
            config.Bind(options);
            return options;
        }

        public static LoggingOptions CreateLoggingOptions(this IConfiguration config)
        {
            var options = new LoggingOptions();
            config.Bind("Logging", options);
            return options;
        }
    }
}