using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.AzureCosmosDb.Tests
{
    [Collection("AzureCosmosDbCollection")]
    public class AzureCosmosDbConnectionProviderTests : TestBase
    {
        public AzureCosmosDbConnectionProviderTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection
                .AddOptions()
                .AddAzureCosmosDbEntityStore(options => new TestConfiguration().ConfigurationRoot.Bind("AzureCosmosDb", options));
        }

        private AzureComsosDbConnectionProvider3 CreateProvider()
        {
            return GetService<AzureComsosDbConnectionProvider3>();
        }


        [Fact]
        public async Task CanClearDatabase()
        {
            var provider = CreateProvider();
            await provider.InitializeAsync(CancellationToken.None);

            var database = provider.Client.GetDatabase(provider.Options.DatabaseId);
            var container = database.GetContainer(provider.Options.CollectionId);

            {
                var dbResponse = await database.ReadAsync();
                dbResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var contRespose = await container.ReadContainerAsync();
                contRespose.StatusCode.Should().Be(HttpStatusCode.OK);
            }


            await provider.ClearDatabaseAsync();

            {
                Func<Task> f1 = async () => { await container.ReadContainerAsync(); };
                f1.Should().Throw<CosmosException>().Where(x=>x.StatusCode == HttpStatusCode.NotFound);

                Func<Task> f2 = async () => { await database.ReadAsync(); };
                f2.Should().Throw<CosmosException>().Where(x=>x.StatusCode == HttpStatusCode.NotFound);
            }


            //await provider.Client.Value.OpenAsync();
            //provider.CheckDatabaseExists().Should().BeTrue();
            //provider.CheckCollectionExists().Should().BeTrue();

            //await provider.ClearDatabaseAsync();
            //provider.CheckDatabaseExists().Should().BeTrue();
            //provider.CheckCollectionExists().Should().BeFalse();

            //await provider.Client.Value.OpenAsync();
            //provider.CheckDatabaseExists().Should().BeTrue();
            //provider.CheckCollectionExists().Should().BeTrue();
        }


        //[Fact]
        //public async Task CanConnectToClient()
        //{
        //    var provider = CreateProvider();
        //    //await provider.Client.Value.OpenAsync();
        //}
    }
}