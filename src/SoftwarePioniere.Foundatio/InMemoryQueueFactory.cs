using System;
using Foundatio.Queues;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.Foundatio
{
    public class InMemoryQueueFactory : IQueueFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public InMemoryQueueFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IQueue<T> CreateQueue<T>(string name) where T : class
        {
            var logger = _loggerFactory.CreateLogger(GetType());
            logger.LogInformation("Create new InMemory Queue {Name} for {Type}", name, typeof(T).Name);

            return new InMemoryQueue<T>(new InMemoryQueueOptions<T>()
            {
                LoggerFactory = _loggerFactory,
                Name = name
            });
        }
    }
}