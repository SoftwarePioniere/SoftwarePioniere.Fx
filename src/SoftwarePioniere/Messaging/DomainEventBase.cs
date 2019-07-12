using System;

namespace SoftwarePioniere.Messaging
{
    public abstract class DomainEventBase : MessageBase, IDomainEvent
    {
        protected DomainEventBase(Guid id, DateTime timeStampUtc, string userId) : base(id, timeStampUtc, userId)
        {

        }
    }
}