using System;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel
{
    public class EmptyDomainEvent : DomainEventBase
    {
        public EmptyDomainEvent() : base(Guid.NewGuid(), DateTime.MinValue, "userId", "aggregateId")
        {

        }

    }
}
