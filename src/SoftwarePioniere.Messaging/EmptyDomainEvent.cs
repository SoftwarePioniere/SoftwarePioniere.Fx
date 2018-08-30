using System;

namespace SoftwarePioniere.Messaging
{
    public class EmptyDomainEvent : DomainEventBase
    {
        public EmptyDomainEvent() : base(Guid.NewGuid(), DateTime.MinValue, "000")
        {

        }

    }
}
