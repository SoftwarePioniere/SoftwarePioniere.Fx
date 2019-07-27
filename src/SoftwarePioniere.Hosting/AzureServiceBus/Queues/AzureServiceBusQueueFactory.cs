using System;
using Foundatio.Messaging;
using Foundatio.Queues;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Hosting.AzureServiceBus.Queues
{
    public class AzureServiceBusQueueFactory : IQueueFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptions<AzureServiceBusMessageBusOptions> _options;

        public AzureServiceBusQueueFactory(ILoggerFactory loggerFactory,
            IOptions<AzureServiceBusMessageBusOptions> options

        )
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public IQueue<T> CreateQueue<T>(string name) where T : class
        {
            var logger = _loggerFactory.CreateLogger(GetType());

            logger.LogInformation("Create new AzureServiceBus Queue {Name} for {Type}", name, typeof(T).Name);

            var opt = _options.Value;

            var queue = new AzureServiceBusQueue<T>(new AzureServiceBusQueueOptions<T>()
            {
                ConnectionString = opt.ConnectionString,
                Name = name,
                LoggerFactory = _loggerFactory
            });

            return queue;
        }
    }
}