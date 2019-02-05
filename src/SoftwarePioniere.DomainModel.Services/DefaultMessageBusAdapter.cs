using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.Services
{
    public class DefaultMessageBusAdapter : IMessageBusAdapter
    {
        private readonly IMessageBus _bus;
        private ILogger _logger;

        public DefaultMessageBusAdapter(ILoggerFactory loggerFactory, IMessageBus bus)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _bus = bus ?? throw new ArgumentNullException(nameof(bus));

            _logger = loggerFactory.CreateLogger(GetType());

        }
        public Task PublishAsync(Type messageType, object message, TimeSpan? delay = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _bus.PublishAsync(messageType, message, delay, cancellationToken);
        }

        public Task PublishAsync<T>(T message, TimeSpan? delay = null,
            CancellationToken cancellationToken = default(CancellationToken), IDictionary<string, string> state = null) where T : class, IMessage
        {
            return _bus.PublishAsync(typeof(T), message, delay, cancellationToken);
        }

        public Task SubscribeMessage<T>(Func<T, IDictionary<string, string>, Task> handler, CancellationToken cancellationToken = default(CancellationToken)) where T : class, IMessage
        {
            return _bus.SubscribeAsync<T>((message, token) => handler(message, null), cancellationToken);
        }

        public Task SubscribeCommand<T>(Func<T, IDictionary<string, string>, Task> handler, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ICommand
        {
            return _bus.SubscribeAsync<T>((message, token) => handler(message, null), cancellationToken);
        }
    }
}
