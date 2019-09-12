using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SoftwarePioniere
{

    public class TestConfiguration
    {
        public TestConfiguration(Action<IConfigurationBuilder> config = null)
        {
            var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..");
            var fullPath = Path.GetFullPath(basePath);
            Console.WriteLine($"TestConfiguration BasePath: {fullPath}");

            // ReSharper disable once UnusedVariable
            var builder = new ConfigurationBuilder()
                //.AddEnvironmentVariables()
                .SetBasePath(fullPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddUserSecrets("sopitest")
                ;

#if !DEBUG
           builder.AddEnvironmentVariables("SOPI_TESTS_");
#endif

            config?.Invoke(builder);

            ConfigurationRoot = builder.Build();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public IConfiguration ConfigurationRoot { get; }

        public T Get<T>(string sectionName)
        {
            return ConfigurationRoot.GetSection(sectionName).Get<T>();
        }

    }
}
