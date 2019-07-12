using FluentAssertions;
using SoftwarePioniere.Messaging;
using Xunit;

namespace SoftwarePioniere.Tests.Messaging
{
    public class FakeEventTest : EventsTestBase<FakeEvent>
    {
        private const string Text = "fake text";

        protected override FakeEvent CreateFromConstructor()
        {
            return new FakeEvent(Id, TimeStamp, UserId, AggregateId, Text);
        }

        protected override void TestIt(FakeEvent o, string s)
        {
            o.UserId.Should().Be(UserId, s);
            o.TimeStampUtc.Should().Be(TimeStamp, s);
            o.Id.Should().Be(Id, s);
         //   o.AggregateId.Should().Be(AggregateId, s);

            o.Text.Should().Be(Text, s);
        }

        [Fact]
        public override void RunTest()
        {
            base.RunTest();
        }
    }
}
