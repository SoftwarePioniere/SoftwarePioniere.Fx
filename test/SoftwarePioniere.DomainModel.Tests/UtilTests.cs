using FluentAssertions;
using SoftwarePioniere.DomainModel.FakeDomain;
using SoftwarePioniere.Messaging;
using Xunit;

namespace SoftwarePioniere.DomainModel.Tests
{
    public class UtilTests
    {


        [Fact]
        public void AggregateIdToStreamNameWithAttributeTest()
        {
            var name = Util.AggregateIdToStreamName(typeof(FakeAggregate), "id1");
            name.Should().Be(string.Concat(Constants.BoundedContextName, "_Fake-ID1"));

            var name2 = Util.AggregateIdToStreamName<FakeAggregate>("id1");
            name2.Should().Be(string.Concat(Constants.BoundedContextName, "_Fake-ID1"));
        }

        [Fact]
        public void AggregateIdToStreamNameWithoutAttributeTest()
        {
            var name = Util.AggregateIdToStreamName(typeof(FakeAggregateWithoutNameAttribute), "id1");
            name.Should().Be("FakeAggregateWithoutNameAttribute-ID1");
        }

        [Fact]
        public void GetAggregateNameWithAttributeTest()
        {
            // ReSharper disable once InvokeAsExtensionMethod
            var name = Util.GetAggregateName(typeof(FakeAggregate));
            name.Should().Be(string.Concat(Constants.BoundedContextName, "_Fake"));
        }

        [Fact]
        public void GetAggregateNameWithoutAttributeTest()
        {
            // ReSharper disable once InvokeAsExtensionMethod
            var name = Util.GetAggregateName(typeof(FakeAggregateWithoutNameAttribute));
            name.Should().Be("FakeAggregateWithoutNameAttribute");
        }

        [Fact]
        public void CanCreateDomainEventMessages()
        {
            var ev = FakeEvent.Create();

            {
                var de = ev.CreateDomainEventMessage("aggname", "aggid");

                de.AggregateId.Should().Be("aggid");
                de.AggregateName.Should().Be("aggname");
                de.DomainEventType.Should().Be("SoftwarePioniere.Messaging.FakeEvent, SoftwarePioniere.Messaging.TestHarness");

                de.Cast<FakeEvent>().Id.Should().Be(ev.Id);
                de.Cast<FakeEvent>().Text.Should().Be(ev.Text);
                de.Cast<FakeEvent>().AggregateId.Should().Be(ev.AggregateId);
                de.Cast<FakeEvent>().TimeStampUtc.Should().Be(ev.TimeStampUtc);
                de.Cast<FakeEvent>().UserId.Should().Be(ev.UserId);
            }

            {
                var de = ev.CreateDomainEventMessageFromType("aggname", "aggid", typeof(FakeEvent));
                
                de.AggregateId.Should().Be("aggid");
                de.AggregateName.Should().Be("aggname");
                de.DomainEventType.Should().Be("SoftwarePioniere.Messaging.FakeEvent, SoftwarePioniere.Messaging.TestHarness");

                de.Cast<FakeEvent>().Id.Should().Be(ev.Id);
                de.Cast<FakeEvent>().Text.Should().Be(ev.Text);
                de.Cast<FakeEvent>().AggregateId.Should().Be(ev.AggregateId);
                de.Cast<FakeEvent>().TimeStampUtc.Should().Be(ev.TimeStampUtc);
                de.Cast<FakeEvent>().UserId.Should().Be(ev.UserId);
            }
        }
    }
}
