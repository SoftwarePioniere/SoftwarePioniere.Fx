using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.Services
{
    public class DefaultMessageBusAdapter : IMessageBusAdapter
    {
        private readonly IMessageBus _bus;

        public DefaultMessageBusAdapter(IMessageBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }
        public Task PublishAsync(Type messageType, object message, TimeSpan? delay = null,
            CancellationToken cancellationToken = default(CancellationToken),
            IDictionary<string, string> state = null)
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

        public async Task SubscribeAggregateEvent<TAggregate, TMessage>(Func<TMessage, AggregateTypeInfo<TAggregate>, IDictionary<string, string>, Task> handler,
            CancellationToken cancellationToken = default(CancellationToken)) where TMessage : class, IDomainEvent
        {
            await _bus.SubscribeAsync<DomainEventMessage>(async (message, token) =>
            {
                if (message.AggregateName == typeof(TAggregate).GetAggregateName())
                {
                    if (message.DomainEventType == typeof(TMessage).GetTypeShortName())
                    {
                        var ev = message.Cast<TMessage>();
                        await handler(ev, new AggregateTypeInfo<TAggregate>(message.AggregateId), null);
                    }

                }
            }, cancellationToken);
        }
    }
}
