using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Hosting;
using StackExchange.Redis;

namespace SoftwarePioniere.Redis
{
    public class RedisConnectionProvider : IConnectionProvider
    {
        private readonly RedisOptions _options;

        public RedisConnectionProvider(IOptions<RedisOptions> options)
        {
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

        private bool _isInitialized;
        private ConnectionMultiplexer _connection;

        private void AssertInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Initialize First");
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            //late initialize
            //https://medium.com/asos-techblog/maximising-net-core-api-performance-11ad883436c

            var connectionstring = _options.ConnectionString;

            if (!string.IsNullOrEmpty(_options.ConnectionString2))
                connectionstring = string.Concat(_options.ConnectionString, _options.ConnectionString2);

            Connection = await ConnectionMultiplexer.ConnectAsync(connectionstring).ConfigureAwait(false);

            _isInitialized = true;
        }
    }
}
