using System;

namespace SoftwarePioniere.Messaging
{
    /// <summary>
    /// Basis Domänen Event
    /// </summary>
    public abstract class DomainEventBase : MessageBase, IDomainEvent
    {
        protected DomainEventBase(Guid id, DateTime timeStampUtc, string userId) : base(id, timeStampUtc, userId)
        {
            // AggregateId = aggregateId;
            // EventVersion = eventVersion;
        }

        ///// <summary>
        ///// Die EventVersion des Events
        ///// </summary>
        //public int EventVersion { get; }

        ///// <summary>
        ///// Die Id des Aggregats, an dem das Event aufgetreten ist
        ///// </summary>
        //public string AggregateId { get; }
    }
}