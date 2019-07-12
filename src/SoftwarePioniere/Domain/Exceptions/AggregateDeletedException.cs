using System;

namespace SoftwarePioniere.Domain.Exceptions
{
    public class AggregateDeletedException : Exception
    {
        public AggregateDeletedException(string aggregateId, Type aggregateType) : base($"Aggregate {aggregateType.FullName} with Id {aggregateId} was deleted")
        {
            AggregateId = aggregateId;
            AggregateType = aggregateType;
        }

        public string AggregateId { get; }
        
        public Type AggregateType { get; }
    }
}