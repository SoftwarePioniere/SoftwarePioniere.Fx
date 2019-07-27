using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Extensions.AspNetCore.AzureAd;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;
using Xunit;

namespace SoftwarePioniere.Extensions.AspNetCore.Tests
{
    public class AzureAdConfigurationTests
    {
        public const string AzureAdResource = "https://testapi.softwarepioniere-demo.de";
        public const string AzureAdAdminGroupId = "975b884d-a98c-4964-90d0-d9aa3c1a0a6c";
        public const string AzureAdTenantId = "74a8c6fa-684f-4b5a-b174-34428871d801";
        public const string AzureAdUserGroupId = "717f59a3-17e0-44a3-9c2d-fbf16e7333d7";
        public const string AzureAdSwaggerClientId = "90b324a8-eff9-4bda-a5e8-19eafc709b10";

        public AzureAdConfigurationTests()
        {

        }

        [Fact]
        public void CanRegisterAzureAdOptions()
        {


            var settings = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("AzureAd:TenantId", AzureAdTenantId),
                new KeyValuePair<string, string>("AzureAd:AdminGroupId", AzureAdAdminGroupId),
                new KeyValuePair<string, string>("AzureAd:Resource", AzureAdResource),
                new KeyValuePair<string, string>("AzureAd:UserGroupId", AzureAdUserGroupId),
                new KeyValuePair<string, string>("AzureAd:SwaggerClientId", AzureAdSwaggerClientId)
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            var services = new ServiceCollection()
                .AddAzureAdOptions(config)
                .AddAzureAdAuthorization()
                ;

            var provider = services.BuildServiceProvider();
            
            var azureAdOpions = provider.GetRequiredService<IOptions<AzureAdOptions>>().Value;

            azureAdOpions.TenantId.Should().Be(AzureAdTenantId);
            azureAdOpions.AdminGroupId.Should().Be(AzureAdAdminGroupId);
            azureAdOpions.Resource.Should().Be(AzureAdResource);
            azureAdOpions.UserGroupId.Should().Be(AzureAdUserGroupId);
            azureAdOpions.SwaggerClientId.Should().Be(AzureAdSwaggerClientId);
            
            var sopiSwaggerOptions = provider.GetRequiredService<IOptions<SopiSwaggerClientOptions>>().Value;

            sopiSwaggerOptions.Resource.Should().Be(AzureAdResource);
            sopiSwaggerOptions.ClientId.Should().Be(AzureAdSwaggerClientId);
        }
    }
}
