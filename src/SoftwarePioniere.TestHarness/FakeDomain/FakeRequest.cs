using System;
using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.FakeDomain
{
    public class FakeRequest : RequestBase
    {
        [J("text")]
        [J1("text")]
        public string Text { get; set; }

        [J("object_id")]
        [J1("object_id")]
        public string ObjectId { get; set; }

        public FakeCommand CreateFakeCommand(string requestId, string userId)
        {
            return new FakeCommand(Guid.NewGuid(), TimeStampUtc, userId, -1, ObjectId, Text);
        }
    }
}