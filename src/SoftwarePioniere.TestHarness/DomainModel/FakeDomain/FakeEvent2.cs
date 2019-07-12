using System;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.FakeDomain
{
    public class FakeEvent2 : DomainEventBase
    {

        public FakeEvent2(Guid id, DateTime timeStampUtc, string userId, string aggregateId, string text) : base(id, timeStampUtc, userId)
        {
            Text = text;
            AggregateId = aggregateId;
        }

        public string Text { get; }
        public string AggregateId { get; }

        public static FakeEvent2 Create()
        {
            return new FakeEvent2(Guid.NewGuid(), DateTime.UtcNow, "fakeuserid", Guid.NewGuid().ToString(), "faketext");

        }
    }
}