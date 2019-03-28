using System;

namespace SoftwarePioniere.Messaging
{
    public class AggregateDomainEventMessage<TAggregate, TDomainEvent> : MessageBase,
        IAggregateDomainEventMessage<TAggregate, TDomainEvent>
        where TDomainEvent : IDomainEvent
        where TAggregate : IAggregateRoot
    {
        public AggregateDomainEventMessage(Guid id, DateTime timeStampUtc, string userId, TDomainEvent domainEventContent, string aggregateId) : base(id, timeStampUtc, userId)
        {
            DomainEventContent = domainEventContent;
            AggregateId = aggregateId;
            DomainEventType = typeof(TDomainEvent).GetTypeShortName();
            AggregateType = typeof(TAggregate).GetTypeShortName();           
        }

        public string AggregateId { get; }
        public string AggregateType { get; }   
        public TDomainEvent DomainEventContent { get; }
        public string DomainEventType { get; }
    }

    public static class AggregateDomainEventMessageExtensions
    {
        public static AggregateDomainEventMessage<TAggregate, TDomainEvent>
            CreateAggregateDomainEventMessage<TAggregate, TDomainEvent>(this TDomainEvent domainEvent,
                TAggregate agg)
            where TDomainEvent : IDomainEvent
            where TAggregate : IAggregateRoot
        {
            var msg = new AggregateDomainEventMessage<TAggregate, TDomainEvent>(
                Guid.NewGuid(),
                domainEvent.TimeStampUtc,
                domainEvent.UserId,
                domainEvent,
                agg.Id
            );

            return msg;
        }
    }
}