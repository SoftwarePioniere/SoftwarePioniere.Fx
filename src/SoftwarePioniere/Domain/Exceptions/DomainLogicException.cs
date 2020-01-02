using System;

namespace SoftwarePioniere.Domain.Exceptions
{

    public class DomainLogicException : SopiException
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
