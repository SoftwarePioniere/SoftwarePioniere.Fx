using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.Clients
{
    public abstract class TokenProviderBase
    {

        private readonly Dictionary<string, JwtSecurityToken> _jwts = new Dictionary<string, JwtSecurityToken>();
        protected readonly ILogger Logger;

        //private readonly object _tokenLock = new object();
        private readonly Dictionary<string, string> _tokens = new Dictionary<string, string>();

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

        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        private async Task AquireNewToken(string audience)
        {
            Logger.LogDebug("Calling AquireNewToken {Audience}", audience);
            
            await SemaphoreSlim.WaitAsync();
            try
            {
                var token = await LoadToken(audience);

                if (_tokens.ContainsKey(audience))
                {
                    _tokens.Remove(audience);
                }

                _tokens.Add(audience, token);

                var jwt = new JwtSecurityToken(token);

                if (_jwts.ContainsKey(audience))
                {
                    _jwts.Remove(audience);
                }

                _jwts.Add(audience, jwt);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                SemaphoreSlim.Release();
            }

            //lock (_tokenLock)
            //{

            //}
        }

        protected abstract Task<string> LoadToken(string resource);

    }
}