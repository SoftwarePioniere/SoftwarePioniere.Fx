using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging
{
    public class FakeRequest : RequestBase
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        public FakeCommand CreateFakeCommand(string requestId, string userId)
        {
            return new FakeCommand(Guid.NewGuid(), TimeStampUtc, userId, -1, Text);
        }
    }

    public class FakeCommand : CommandBase
    {
        public FakeCommand(Guid id, DateTime timeStampUtc, string userId, int originalVersion,
            string text) : base(id,
            timeStampUtc,
            userId,
            originalVersion,
            "fakeobject",
            id.ToString()
        )
        {
            Text = text;
        }

        public string Text { get; }
    }

    public class FakeEvent : DomainEventBase
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
            return new FakeEvent(Guid.NewGuid(), DateTime.UtcNow, "fakeuserid", Guid.NewGuid().ToString(), "faketext");
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