using System;

namespace SoftwarePioniere.Domain
{

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AggregateNameAttribute : Attribute
    {
        public string BoundedContext { get; }
        public string Aggregate { get; }

        public AggregateNameAttribute(string boundedContext, string aggregate)
        {
            BoundedContext = boundedContext;
            Aggregate = aggregate;

            AggregateStreamName = $"{boundedContext}_{aggregate}";
        }

        public string AggregateStreamName { get; }
    }
}
