using System;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.FakeDomain
{
    public class FakeCommand : CommandBase
    {
        public FakeCommand(Guid id, DateTime timeStampUtc, string userId, int originalVersion, string objectId,
            string text) : base(id,
            timeStampUtc,
            userId,
            originalVersion,
            "fakeobject",
            objectId)
        {
            Text = text;
        }

        public string Text { get; }
    }
}