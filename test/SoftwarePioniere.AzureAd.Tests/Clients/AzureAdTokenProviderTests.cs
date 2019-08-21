using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.AzureAd.Clients;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.AzureAd.Tests.Clients
{
    public class AzureAdTokenProviderTests : TestBase
    {
        public AzureAdTokenProviderTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection
                .AddOptions()
                .AddAzureAdClientOptions(new TestConfiguration().ConfigurationRoot)
                .AddSingleton<AzureAdTokenProvider>()
                ;
        }

        [Fact]
        public async Task GetAccessTokenTest()
        {
            var provider = GetService<AzureAdTokenProvider>();
            var token = await provider.GetAccessToken("https://testapi.softwarepioniere-demo.de");

            token.Should().NotBeNullOrEmpty();

        }

    }
}
