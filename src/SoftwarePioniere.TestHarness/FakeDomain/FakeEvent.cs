using System;
using System.Collections.Generic;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.FakeDomain
{
    public interface IFakeAggregateIdEvent
    {
        string AggregateId { get; }
    }

    public class FakeEvent : DomainEventBase, IFakeAggregateIdEvent
    {
        public FakeEvent(Guid id, DateTime timeStampUtc, string userId, string aggregateId, string text) : base(id,
            timeStampUtc,
            userId)
        {
            Text = text;
            AggregateId = aggregateId;
        }

        public string AggregateId { get; }

        public string Text { get; }

        public static FakeEvent Create()
        {
            return new FakeEvent(Guid.NewGuid(), DateTime.UtcNow, "fakeuserid",Guid.NewGuid().ToString(), "faketext");
        }

        public static IEnumerable<FakeEvent> CreateList(int count)
        {
            var id = Guid.NewGuid().ToString();
            for (var i = 0;
                i < count;
                i++)
                yield return new FakeEvent(Guid.NewGuid(), DateTime.UtcNow, "fakeuserid", id, $"faketext {i + 1}");
        }
    }
}