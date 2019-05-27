using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Messaging
{
    public interface IMessageBusAdapter
    {
        Task PublishAsync(
            Type messageType,
            object message,
            TimeSpan? delay = null,
            CancellationToken cancellationToken = default(CancellationToken)
            //,IDictionary<string, string> state = null
            );

        Task PublishAsync<T>(
            T message,
            TimeSpan? delay = null,
            CancellationToken cancellationToken = default(CancellationToken)
        //,IDictionary<string, string> state = null
        )
            where T : class, IMessage;

        Task SubscribeMessage<T>(Func<T,
                //IDictionary<string, string>, 
                Task> handler,
            CancellationToken cancellationToken = default(CancellationToken))
            where T : class, IMessage;

        Task SubscribeCommand<T>(Func<T,
                //IDictionary<string, string>, 
                Task> handler,
            CancellationToken cancellationToken = default(CancellationToken))
            where T : class, ICommand;

        Task SubscribeAggregateDomainEvent<TAggregate, TDomainEvent>(Func<TDomainEvent, AggregateTypeInfo<TAggregate>,
                //IDictionary<string, string>, 
                Task> handler,
            CancellationToken cancellationToken = default(CancellationToken))
            where TDomainEvent : class, IDomainEvent
            where TAggregate : IAggregateRoot;

        //Task SubscribeAggregateEvent<TAggregate, TMessage>(
        //    Func<TMessage, AggregateTypeInfo<TAggregate>, IDictionary<string, string>, Task> handler,
        //    CancellationToken cancellationToken = default(CancellationToken))
        //    where TMessage : class, IDomainEvent
        //    where TAggregate : IAggregateRoot;

        Task<MessageResponse> PublishCommandAsync<T>(T cmd, CancellationToken cancellationToken = default(CancellationToken)
            //, IDictionary<string, string> state = null
            ) where T : class, ICommand;

        Task<MessageResponse> PublishCommandsAsync<T>(IEnumerable<T> cmds, CancellationToken cancellationToken = default(CancellationToken)
            //,IDictionary<string, string> state = null
            ) where T : class, ICommand;

    }
}