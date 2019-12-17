using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SoftwarePioniere.MongoDb.Tests
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
                .AddMongoDbEntityStoreOptions(options => new TestConfiguration().ConfigurationRoot.Bind("MongoDb", options))
                .AddMongoDbEntityStore();

            var provider = services.BuildServiceProvider().GetService<MongoDbConnectionProvider>();
            provider.ClearDatabaseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}