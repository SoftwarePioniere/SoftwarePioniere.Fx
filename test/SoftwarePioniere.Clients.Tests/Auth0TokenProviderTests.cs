using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Clients.Auth0;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.Clients.Tests
{
    public class Auth0TokenProviderTests : TestBase
    {
        public Auth0TokenProviderTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection
                .AddOptions()
                .AddAuth0ClientOptions(new TestConfiguration().ConfigurationRoot)
                .AddSingleton<Auth0TokenProvider>()
                ;
        }

        [Fact]
        public async Task GetAccessTokenTest()
        {
            var provider = GetService<Auth0TokenProvider>();
            var token = await provider.GetAccessToken("https://testapi.softwarepioniere-demo.de");

            token.Should().NotBeNullOrEmpty();

        }

    }
}
