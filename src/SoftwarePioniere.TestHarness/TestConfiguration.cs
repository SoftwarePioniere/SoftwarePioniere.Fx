using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SoftwarePioniere
{

    public class TestConfiguration
    {
        public TestConfiguration(Action<IConfigurationBuilder> config = null)
        {
            Console.WriteLine($"AppContext.BaseDirectory: {AppContext.BaseDirectory}");
            var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..");
            Console.WriteLine($"Base Path: {basePath}");
            var fullPath = Path.GetFullPath(basePath);
            Console.WriteLine($"FullBasePath: {fullPath}");

            // ReSharper disable once UnusedVariable
            var builder = new ConfigurationBuilder()
                //.AddEnvironmentVariables()
                .SetBasePath(fullPath)
                .AddJsonFile("appsettings.secret.json", true, true)
                .AddJsonFile("appsettings.json", true, true);

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
