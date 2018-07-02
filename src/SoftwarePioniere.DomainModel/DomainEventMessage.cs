using System;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel
{

   
    /// <summary>
    /// Nachricht um ein Domain Even
    /// von einem bestimmten Aggregat zu verschicken
    /// </summary>    
    public class DomainEventMessage<TDomainEvent> : MessageBase where TDomainEvent : IDomainEvent
    {

        public DomainEventMessage(Guid id, DateTime timeStampUtc, string userId
            , string aggregateName, string aggregateId, string domainEventType, TDomainEvent domainEvent
        ) : base(id, timeStampUtc, userId)
        {
            DomainEvent = domainEvent;
            AggregateName = aggregateName;
            AggregateId = aggregateId;
            DomainEventType = domainEventType;
        }

        public TDomainEvent DomainEvent { get; }

        public string AggregateName { get; }
        public string AggregateId { get; }
        public string DomainEventType { get; }
    }

    ///// <summary>
    ///// Nachricht um ein Domain Even
    ///// von einem bestimmten Aggregat zu verschicken
    ///// </summary>    
    //public sealed class DomainEventMessage<TDomainEvent> : DomainEventMessage where TDomainEvent:IDomainEvent
    //{

    //    public DomainEventMessage(Guid id, DateTime timeStampUtc, string userId
    //        , string aggregateName, string aggregateId, TDomainEvent domainEvent
    //        ) : base(id, timeStampUtc, userId, aggregateName, aggregateId,  typeof(TDomainEvent).GetTypeShortName() , domainEvent )
    //    {
            
    //    }
              
    //}

}
