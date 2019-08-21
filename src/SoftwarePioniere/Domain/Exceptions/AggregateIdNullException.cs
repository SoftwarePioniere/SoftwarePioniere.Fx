using System;

namespace SoftwarePioniere.Domain.Exceptions
{
    public class EmptyAggregateIdException : Exception
    {
        public Type AggregateType { get; }

        public EmptyAggregateIdException(Type aggregateType) : base($"Empty Id in Aggregate {aggregateType.FullName}")
        {
            AggregateType = aggregateType;
        }
    }
}
