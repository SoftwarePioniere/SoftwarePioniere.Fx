using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Hosting;
using StackExchange.Redis;

namespace SoftwarePioniere.Redis
{
    public class RedisConnectionProvider : IConnectionProvider
    {
        private readonly ILogger _logger;
        private readonly RedisOptions _options;
        private ConnectionMultiplexer _connection;

        private bool _isInitialized;

        public RedisConnectionProvider(ILoggerFactory loggerFactory, IOptions<RedisOptions> options)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger(GetType());

            _logger.LogInformation("Redis Options {@Options}", options.Value);
            _options = options.Value;
        }

        public ConnectionMultiplexer Connection
        {
            get
            {
                AssertInitialized();
                return _connection;
            }
            private set => _connection = value;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            //late initialize
            //https://medium.com/asos-techblog/maximising-net-core-api-performance-11ad883436c

            var connectionstring = _options.ConnectionString;

            if (!string.IsNullOrEmpty(_options.ConnectionString2))
            {
                connectionstring = string.Concat(_options.ConnectionString, _options.ConnectionString2);
            }
            
            _logger.LogInformation("ConnectAsync with connectionString: {ConnectionString}", connectionstring);

            Connection = await ConnectionMultiplexer.ConnectAsync(connectionstring).ConfigureAwait(false);

            _isInitialized = true;
        }

        private void AssertInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Initialize First");
            }
        }
    }
}