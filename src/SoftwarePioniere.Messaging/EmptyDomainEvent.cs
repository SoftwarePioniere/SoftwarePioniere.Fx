using System;

namespace SoftwarePioniere.Messaging
{
    public class EmptyDomainEvent : MessageBase
    {
        public EmptyDomainEvent() : base(Guid.NewGuid(), DateTime.MinValue, "userId")
        {

        }

    }
}
