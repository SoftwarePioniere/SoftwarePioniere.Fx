using System;
using Newtonsoft.Json;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.FakeDomain
{
    public class FakeRequest : RequestBase
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("object_id")]
        public string ObjectId { get; set; }

        public FakeCommand CreateFakeCommand(string requestId, string userId)
        {
            return new FakeCommand(Guid.NewGuid(), TimeStampUtc, userId, -1, ObjectId, Text);
        }
    }
}