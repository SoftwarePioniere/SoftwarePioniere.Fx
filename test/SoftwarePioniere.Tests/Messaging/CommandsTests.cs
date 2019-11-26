using System;
using FluentAssertions;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.Messaging;
using Xunit;

namespace SoftwarePioniere.Tests.Messaging
{

    public class FakeCommandTest : CommandsTestBase<FakeCommand>
    {
        private const string Text = "fake text";

        protected override FakeCommand CreateFromConstructor()
        {
            return new FakeCommand(Id, TimeStamp, UserId, OriginalVersion, Id.ToString(), Text);
        }

        protected override void TestIt(FakeCommand o, string s)
        {
            o.UserId.Should().Be(UserId, s);
            //o.GetRequestId().Should().Be(RequestId, s);
            o.TimeStampUtc.Should().Be(TimeStamp, s);
            //o.ObjectId.S

            if (s != FromRequest)
            {
                o.Id.Should().Be(Id, s);
                o.OriginalVersion.Should().Be(OriginalVersion, RequestId, s);
            }

            o.Text.Should().Be(Text, s);
        }

        [Fact]
        public override void RunTest()
        {
            base.RunTest();

            var request = new FakeRequest
            {
                Text = Text,
                TimeStampUtc = TimeStamp,
                ObjectId = Guid.NewGuid().ToString()
            };

            var cmd = request.CreateFakeCommand(RequestId, UserId);
            TestCreatedFromRequest(cmd);
        }
    }
}
