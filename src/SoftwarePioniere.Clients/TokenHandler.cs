using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.Clients
{
    /// <summary>
    /// Token in Client Request einfügen
    /// </summary>
    public class TokenHandler : DelegatingHandler
    {
        private readonly ITokenProvider _tokenProvider;
        private readonly string _audience;
        private readonly ILogger _logger;

        /// <inheritdoc />
        public TokenHandler(ILoggerFactory loggerFactory, ITokenProvider tokenProvider, string audience)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger(GetType());
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _audience = audience ?? throw new ArgumentNullException(nameof(audience));
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains("Authorization"))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Adding Auth Header");
                }

                var token = await _tokenProvider.GetAccessToken(_audience);
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            return await base.SendAsync(request, cancellationToken);

        }
    }
}