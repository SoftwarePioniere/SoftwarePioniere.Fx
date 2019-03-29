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
            CancellationToken cancellationToken = default,
            IDictionary<string, string> state = null)
        {
            return _bus.PublishAsync(messageType, message, delay, cancellationToken);
        }

        public Task PublishAsync<T>(T message, TimeSpan? delay = null,
            CancellationToken cancellationToken = default, IDictionary<string, string> state = null) where T : class, IMessage
        {
            return _bus.PublishAsync(typeof(T), message, delay, cancellationToken);
        }

        public Task SubscribeMessage<T>(Func<T, IDictionary<string, string>, Task> handler, CancellationToken cancellationToken = default) where T : class, IMessage
        {
            return _bus.SubscribeAsync<T>((message, token) => handler(message, null), cancellationToken);
        }

        public Task SubscribeCommand<T>(Func<T, IDictionary<string, string>, Task> handler, CancellationToken cancellationToken = default) where T : class, ICommand
        {
            return _bus.SubscribeAsync<T>((message, token) => handler(message, null), cancellationToken);
        }

        public async Task SubscribeAggregateDomainEvent<TAggregate, TDomainEvent>(Func<TDomainEvent, AggregateTypeInfo<TAggregate>, IDictionary<string, string>, Task> handler,
            CancellationToken cancellationToken = default) where TAggregate : IAggregateRoot where TDomainEvent : class, IDomainEvent
        {
            await _bus.SubscribeAsync<AggregateDomainEventMessage>(async (message, token) =>
             {
                 if (message.IsAggregate<TAggregate>() && message.IsEventType<TDomainEvent>())
                 {
                     await handler(message.GetEvent<TDomainEvent>(), new AggregateTypeInfo<TAggregate>(message.AggregateId), null);
                 }

             }, cancellationToken);
        }

        public async Task<MessageResponse> PublishCommandAsync<T>(T cmd, CancellationToken cancellationToken = default,
            IDictionary<string, string> state = null) where T : class, ICommand
        {
            await PublishAsync(cmd, TimeSpan.Zero, cancellationToken, state);
            return new MessageResponse
            {
                MessageId = cmd.Id,
                UserId = cmd.UserId
            };
        }

        public async Task<MessageResponse> PublishCommandsAsync<T>(IEnumerable<T> cmds, CancellationToken cancellationToken = default,
            IDictionary<string, string> state = null) where T : class, ICommand
        {
            MessageResponse response = null;

            foreach (var cmd in cmds)
            {
                response = await PublishCommandAsync(cmd, cancellationToken, state);
            }

            return response;
        }
    }
}
