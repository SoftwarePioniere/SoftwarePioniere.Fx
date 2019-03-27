using System;
using Foundatio.Queues;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SoftwarePioniere.Foundatio.Redis
{
    public class RedisQueueFactory : IQueueFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConnectionMultiplexer _connection;

        public RedisQueueFactory(ILoggerFactory loggerFactory, ConnectionMultiplexer connection)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _connection = connection;
        }

        public IQueue<T> CreateQueue<T>(string name) where T : class
        {
            var logger = _loggerFactory.CreateLogger(GetType());

            logger.LogInformation("Create new Redis Queue {Name} for {Type}", name, typeof(T).Name);
            var queue = new RedisQueue<T>(new RedisQueueOptions<T>()
            {
                LoggerFactory = _loggerFactory,
                Name = name,                                       
                ConnectionMultiplexer = _connection
            });

            return queue;
        }
    }
}
