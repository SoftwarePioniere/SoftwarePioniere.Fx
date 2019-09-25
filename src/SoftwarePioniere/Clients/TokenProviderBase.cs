using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.Clients
{
    public abstract class TokenProviderBase
    {

        private readonly ConcurrentDictionary<string, JwtSecurityToken> _jwts = new ConcurrentDictionary<string, JwtSecurityToken>();
        protected readonly ILogger Logger;
        private readonly ConcurrentDictionary<string, string> _tokens = new ConcurrentDictionary<string, string>();

        protected TokenProviderBase(ILoggerFactory loggerFactory)
        {

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            Logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task<string> GetAccessToken(string audience)
        {
            Logger.LogDebug("GetAccessToken {Audience}", audience);

            _jwts.TryGetValue(audience, out var jwt);

            if (jwt == null)
            {
                Logger.LogDebug("No Token, do AquireNewToken");

                await AquireNewToken(audience);
                return _tokens[audience];
            }

            if (jwt.ValidTo <= DateTime.UtcNow.AddMinutes(-2))
            {
                Logger.LogDebug("Token will expire, do AquireNewToken");

                await AquireNewToken(audience);
                return _tokens[audience];
            }

            return _tokens[audience];
        }

        private async Task AquireNewToken(string audience)
        {
            Logger.LogDebug("Calling AquireNewToken {Audience}", audience);

            var token = await LoadToken(audience);

            _tokens.AddOrUpdate(audience, token, (s, s1) => token);
            var jwt = new JwtSecurityToken(token);
            _jwts.AddOrUpdate(audience, jwt, (s, securityToken) => jwt);
        }

        protected abstract Task<string> LoadToken(string resource);

    }
}