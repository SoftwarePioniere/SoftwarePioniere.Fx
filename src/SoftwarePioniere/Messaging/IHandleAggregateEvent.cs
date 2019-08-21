using System;
using System.Threading.Tasks;

namespace SoftwarePioniere.Messaging
{
    public interface IHandleAggregateEvent<TAggregate, in TMessage> where TMessage : IMessage, IDomainEvent
    {
        Task HandleAsync(TMessage message, AggregateTypeInfo<TAggregate> info);
    }

    public class AggregateTypeInfo<TAggregate>
    {
        public AggregateTypeInfo(string aggregateId)
        {
            AggregateId = aggregateId;
            AggregateType = typeof(TAggregate);
        }

        public Type AggregateType { get; private set; }

        public string AggregateId { get; private set; }
    }
}
