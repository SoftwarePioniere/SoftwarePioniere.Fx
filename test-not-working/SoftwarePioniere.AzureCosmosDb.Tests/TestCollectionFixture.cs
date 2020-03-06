using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SoftwarePioniere.AzureCosmosDb.Tests
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestCollectionFixture : IDisposable
    {
        public void Dispose()
        {
            var services = new ServiceCollection();
            services
                .AddOptions()
                .AddSingleton<ILoggerFactory>(new NullLoggerFactory())
                .AddAzureCosmosDbEntityStore(options => new TestConfiguration().ConfigurationRoot.Bind("AzureCosmosDb", options));

            var provider = services.BuildServiceProvider().GetRequiredService<AzureComsosDbConnectionProvider3>();
            provider.ClearDatabaseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            //provider.DeleteDocumentCollectionAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}