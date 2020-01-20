using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.MongoDb.Tests
{
    [Collection("MongoDbCollection")]
    public class MongoDbConnectionProviderTests  : TestBase
    {
        
        private MongoDbConnectionProvider CreateProvider()
        {
            return GetService<MongoDbConnectionProvider>();
        }

        public MongoDbConnectionProviderTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection
                .AddOptions()
                .AddMongoDbEntityStoreOptions(options => new TestConfiguration().ConfigurationRoot.Bind("MongoDb", options))
                .AddMongoDbEntityStore();
        }

        [Fact]
        public async Task CanConnectToClient()
        {
            var provider = CreateProvider();
            await provider.InitializeAsync(CancellationToken.None);
            await provider.CreateClient().ListDatabasesAsync();
        }

        [Fact]
        public async Task CanClearDatabase()
        {
            var provider = CreateProvider();
            await provider.InitializeAsync(CancellationToken.None);


            await provider.Database.Value.CreateCollectionAsync(Guid.NewGuid().ToString());
            (await provider.CheckDatabaseExistsAsync()).Should().BeTrue();

            await provider.ClearDatabaseAsync();
            (await provider.CheckDatabaseExistsAsync()).Should().BeFalse();

            await provider.Database.Value.CreateCollectionAsync(Guid.NewGuid().ToString());
            (await provider.CheckDatabaseExistsAsync()).Should().BeTrue();

        }
    }
}
