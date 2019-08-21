using System;
using Newtonsoft.Json;
using SoftwarePioniere.Domain;

namespace SoftwarePioniere.Messaging
{
    public class AggregateDomainEventMessage : MessageBase,
        IAggregateDomainEventMessage
    {
        public AggregateDomainEventMessage(Guid id, DateTime timeStampUtc, string userId, string aggregateId, string aggregateType, string domainEventContent, string domainEventType) : base(id, timeStampUtc, userId)
        {
            AggregateId = aggregateId;
            AggregateType = aggregateType;
            DomainEventContent = domainEventContent;
            DomainEventType = domainEventType;
        }

        public string AggregateId { get; }
        public string AggregateType { get; }
        public string DomainEventContent { get; }
        public string DomainEventType { get; }
    }

    //public class AggregateDomainEventMessage<TAggregate, TDomainEvent> : MessageBase,
    //    IAggregateDomainEventMessage<TAggregate, TDomainEvent>
    //    where TDomainEvent : IDomainEvent
    //    where TAggregate : IAggregateRoot
    //{
    //    public AggregateDomainEventMessage(Guid id, DateTime timeStampUtc, string userId, TDomainEvent domainEventContent, string aggregateId) : base(id, timeStampUtc, userId)
    //    {
    //        DomainEventContent = domainEventContent;
    //        AggregateId = aggregateId;
    //        DomainEventType = typeof(TDomainEvent).GetTypeShortName();
    //        AggregateType = typeof(TAggregate).GetTypeShortName();           
    //    }

    //    public string AggregateId { get; }
    //    public string AggregateType { get; }   
    //    public TDomainEvent DomainEventContent { get; }
    //    public string DomainEventType { get; }
    //}

    public static class AggregateDomainEventMessageExtensions
    {
        //public static AggregateDomainEventMessage<TAggregate, TDomainEvent>
        //    CreateAggregateDomainEventMessage<TAggregate, TDomainEvent>(this TDomainEvent domainEvent,
        //        TAggregate agg)
        //    where TDomainEvent : IDomainEvent
        //    where TAggregate : IAggregateRoot
        //{
        //    var msg = new AggregateDomainEventMessage<TAggregate, TDomainEvent>(
        //        Guid.NewGuid(),
        //        domainEvent.TimeStampUtc,
        //        domainEvent.UserId,
        //        domainEvent,
        //        agg.Id
        //    );

        //    return msg;
        //}

        public static AggregateDomainEventMessage CreateAggregateDomainEventMessage<TAggregate>(this IDomainEvent domainEvent,
            TAggregate aggregate)
            where TAggregate : IAggregateRoot
        {
            return new AggregateDomainEventMessage(Guid.NewGuid(), domainEvent.TimeStampUtc, domainEvent.UserId, aggregate.Id,
                typeof(TAggregate).GetTypeShortName(), JsonConvert.SerializeObject(domainEvent), domainEvent.GetType().GetTypeShortName()
                );

            //var typeArgument1 = typeof(TAggregate).GetTypeShortName();
            //var typeArgument2 = domainEvent.GetType();

            //var genericClass = typeof(AggregateDomainEventMessage<,>);
            //var constructedClass = genericClass.MakeGenericType(typeArgument1, typeArgument2);

            //// public AggregateDomainEventMessage(Guid id, DateTime timeStampUtc, string userId, TDomainEvent domainEventContent, string aggregateId) : base(id, timeStampUtc, userId)                        
            //var created = Activator.CreateInstance(constructedClass,
            //    Guid.NewGuid(), domainEvent.TimeStampUtc, domainEvent.UserId,
            //    domainEvent, aggregate.Id);

            //return created;
        }

        public static bool IsEventType<T>(this AggregateDomainEventMessage item)
            where T : IDomainEvent
        {
            return typeof(T).GetTypeShortName().Equals(item.DomainEventType);
        }


        public static bool IsAggregate<T>(this AggregateDomainEventMessage item)
            where T : IAggregateRoot
        {
            return typeof(T).GetTypeShortName().Equals(item.AggregateType);
        }

        public static T GetEvent<T>(this AggregateDomainEventMessage item)
            where T : class, IDomainEvent
        {
            if (item.IsEventType<T>())
            {
                return JsonConvert.DeserializeObject<T>(item.DomainEventContent);
            }
            return default(T);
        }
    }
}