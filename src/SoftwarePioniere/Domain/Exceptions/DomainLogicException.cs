using System;

namespace SoftwarePioniere.Domain.Exceptions
{

    public class DomainLogicException : Exception
    {
        [Obsolete]
        public DomainLogicException(string message) : base(message)
        {
            
        }

        public DomainLogicException(AggregateRoot aggregateRoot, string message) : base(message)
        {
            AggregateId = aggregateRoot?.Id;
            AggregateType = aggregateRoot?.GetType();
        }

        public string AggregateId { get; }

        public Type AggregateType { get; }
    }
}
