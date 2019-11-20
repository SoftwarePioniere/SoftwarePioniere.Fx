using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SoftwarePioniere.Clients;

namespace SoftwarePioniere.AzureAd.Clients
{
    public class AzureAdTokenProvider : TokenProviderBase, ITokenProvider
    {
        private readonly ConcurrentDictionary<string, AuthenticationContext> _contexte = new ConcurrentDictionary<string, AuthenticationContext>();
        private readonly AzureAdClientOptions _settings;
        private readonly string _emptyTenantId = Guid.NewGuid().ToString();

        public AzureAdTokenProvider(ILoggerFactory loggerFactory, IOptions<AzureAdClientOptions> options) : base(loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _settings = options.Value;


        }



        protected override async Task<string> LoadToken(string resource, string tenantId)
        {
            Logger.LogInformation("Loading AzureAd Token for {Resource}", resource);

            string authority;

            if (string.IsNullOrEmpty(tenantId))
            {
                authority = _settings.Authority;
                tenantId = _emptyTenantId;
            }
            else
            {
                authority = $"https://login.microsoftonline.com/{tenantId}";
            }

            var context = _contexte.GetOrAdd(tenantId, new AuthenticationContext(authority));

            var authenticationResult = await context.AcquireTokenAsync(resource,
                new ClientCredential(_settings.ClientId, _settings.ClientSecret));

            return authenticationResult.AccessToken;
        }
    }
}
