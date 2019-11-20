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

        public async Task<string> GetAccessToken(string audience, bool force, string tenantId = "")
        {
            Logger.LogDebug("GetAccessToken {Audience} {Force}", audience, force);

            var tokenKey = $"{audience}-{tenantId}";

            if (force && _jwts.ContainsKey(tokenKey))
            {
                _jwts.TryRemove(tokenKey, out var jj);
                Logger.LogDebug("Forced Remove Token {Token}", jj?.Id);
            }

            _jwts.TryGetValue(tokenKey, out var jwt);

            if (jwt == null)
            {
                Logger.LogDebug("No Token, do AquireNewToken");

                await AquireNewToken(audience, tenantId);
                return _tokens[tokenKey];
            }

            if (jwt.ValidTo <= DateTime.UtcNow.AddMinutes(-2))
            {
                Logger.LogDebug("Token will expire, do AquireNewToken");

                await AquireNewToken(audience, tenantId);
                return _tokens[tokenKey];
            }

            return _tokens[tokenKey];
        }

        public Task<string> GetAccessToken(string audience, string tenantId = "")
        {
            return GetAccessToken(audience, false, tenantId);
        }

        private async Task AquireNewToken(string audience, string tenantId)
        {
            Logger.LogDebug("Calling AquireNewToken {Audience}", audience);

            var tokenKey = $"{audience}-{tenantId}";

            var token = await LoadToken(audience, tenantId);

            _tokens.AddOrUpdate(tokenKey, token, (key, value) => token);
            var jwt = new JwtSecurityToken(token);
            _jwts.AddOrUpdate(tokenKey, jwt, (s, securityToken) => jwt);
        }

        protected abstract Task<string> LoadToken(string resource, string tenantId);

    }
}