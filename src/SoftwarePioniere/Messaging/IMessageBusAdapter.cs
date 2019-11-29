using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SoftwarePioniere.Domain;

namespace SoftwarePioniere.Messaging
{
    public interface IMessageBusAdapter
    {
        Task PublishAsync(
            Type messageType,
            object message,
            TimeSpan? delay = null,
            CancellationToken cancellationToken = default);

        Task PublishAsync<T>(
            T message,
            TimeSpan? delay = null,
            CancellationToken cancellationToken = default)
            where T : class, IMessage;

     //   Task<MessageResponse> PublishCommandAsync<T>(T cmd) where T : class, ICommand;

        Task<MessageResponse> PublishCommandAsync<T>(T cmd, CancellationToken cancellationToken = default) where T : class, ICommand;

        Task<MessageResponse> PublishCommandsAsync<T>(IEnumerable<T> cmds, CancellationToken cancellationToken = default) where T : class, ICommand;

        Task SubscribeAggregateDomainEvent<TAggregate, TDomainEvent>(Func<TDomainEvent, AggregateTypeInfo<TAggregate>,
                Task> handler,
            CancellationToken cancellationToken = default
            , Func<TDomainEvent, AggregateTypeInfo<TAggregate>, string> lockId = null
        )
            where TDomainEvent : class, IDomainEvent
            where TAggregate : IAggregateRoot;

        Task SubscribeCommand<T>(Func<T,
                Task> handler,
            CancellationToken cancellationToken = default
            , Func<T, string> lockId = null
        )
            where T : class, ICommand;

        Task SubscribeMessage<T>(Func<T, Task> handler,
            CancellationToken cancellationToken = default
            , Func<T, string> lockId = null
        ) where T : class, IMessage;
    }
}