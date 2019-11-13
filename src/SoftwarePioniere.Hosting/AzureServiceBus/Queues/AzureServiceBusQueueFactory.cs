using System;
using Foundatio.Messaging;
using Foundatio.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Hosting.AzureServiceBus.Queues
{
    public class AzureServiceBusQueueFactory : IQueueFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly AzureServiceBusMessageBusOptions _messageBusOptions;
        private readonly IServiceProvider _serviceProvider;

        public AzureServiceBusQueueFactory(ILoggerFactory loggerFactory,
            IOptions<AzureServiceBusMessageBusOptions> options,
            IServiceProvider serviceProvider

        )
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _messageBusOptions = options.Value;
        }
        public IQueue<T> CreateQueue<T>(string name) where T : class
        {
            var logger = _loggerFactory.CreateLogger(GetType());

            logger.LogInformation("Create new AzureServiceBus Queue {Name} for {Type}", name, typeof(T).Name);

            var iopt = _serviceProvider.GetService<IOptions<AzureServiceBusQueueOptions<T>>>();

            if (iopt == null)
                throw new InvalidOperationException($"Please Register AzureServiceBusQueueOptions {typeof(T).FullName}");

            var options = iopt.Value;
            options.ConnectionString = _messageBusOptions.ConnectionString;
            options.Name = name;
            options.LoggerFactory = _loggerFactory;

            var queue = new AzureServiceBusQueue<T>(options);

            return queue;
        }
    }
}