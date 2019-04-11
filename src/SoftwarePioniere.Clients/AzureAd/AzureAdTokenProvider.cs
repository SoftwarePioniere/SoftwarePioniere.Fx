using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace SoftwarePioniere.Clients.AzureAd
{
    public class AzureAdTokenProvider : TokenProviderBase, ITokenProvider
    {
        private readonly AzureAdClientOptions _settings;
        private readonly AuthenticationContext _context;

        public AzureAdTokenProvider(ILoggerFactory loggerFactory, IOptions<AzureAdClientOptions> options) : base(loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _settings = options.Value;

            _context = new AuthenticationContext(_settings.Authority);
        }

        protected override async Task<string> LoadToken(string resource)
        {
            Logger.LogInformation("Loading AzureAd Token for {Resource}", resource);          

            var authenticationResult = await _context.AcquireTokenAsync(resource,
                new ClientCredential(_settings.ClientId, _settings.ClientSecret));

            return authenticationResult.AccessToken;
        }
    }
}
