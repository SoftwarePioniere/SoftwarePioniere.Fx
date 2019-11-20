using System;
using System.Threading.Tasks;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Clients;

namespace SoftwarePioniere.Auth0.Clients
{
    public class Auth0TokenProvider : TokenProviderBase, ITokenProvider
    {
        private readonly Auth0ClientOptions _settings;

        public Auth0TokenProvider(ILoggerFactory loggerFactory, IOptions<Auth0ClientOptions> options)
        : base(loggerFactory)
        {

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _settings = options.Value;
        }

        protected override async Task<string> LoadToken(string resource, string tenantId)
        {
            Logger.LogInformation("Loading Auth0 Token for {Resource}", resource);

            var client = new AuthenticationApiClient(new Uri($"https://{_settings.TenantId}"));
            var tokenResponse = await client.GetTokenAsync(new ClientCredentialsTokenRequest
            {
                Audience = resource, // _settings.Audience,
                ClientId = _settings.ClientId,
                ClientSecret = _settings.ClientSecret
            });
            return tokenResponse.AccessToken;
        }
    }
}