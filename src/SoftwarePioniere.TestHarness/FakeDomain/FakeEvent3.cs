using System;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.FakeDomain
{
    public class FakeEvent3 : DomainEventBase, IFakeAggregateIdEvent
    {

        public string AggregateId { get; }

        public FakeEvent3(Guid id, DateTime timeStampUtc, string userId, string aggregateId) : base(id, timeStampUtc, userId)
        {
            AggregateId = aggregateId;
        }

    }
}