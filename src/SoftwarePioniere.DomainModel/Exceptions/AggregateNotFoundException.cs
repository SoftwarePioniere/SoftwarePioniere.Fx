using System;

namespace SoftwarePioniere.DomainModel.Exceptions
{
    
    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException(string aggregateId, Type aggregateType) : base($"Aggregate {aggregateType.FullName} with Id {aggregateId} was not found")
        {
            AggregateId = aggregateId;
            AggregateType = aggregateType;
        }

        public string AggregateId { get; }
        
        public Type AggregateType { get; }

    
    }
}