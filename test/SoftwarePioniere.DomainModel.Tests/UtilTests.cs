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
                de.DomainEventType.Should().Be("FakeEvent");

                de.DomainEvent.Id.Should().Be(ev.Id);
                de.DomainEvent.Text.Should().Be(ev.Text);
                de.DomainEvent.AggregateId.Should().Be(ev.AggregateId);
                de.DomainEvent.TimeStampUtc.Should().Be(ev.TimeStampUtc);
                de.DomainEvent.UserId.Should().Be(ev.UserId);
            }

            {
                var detemp = ev.CreateDomainEventMessageFromType("aggname", "aggid");
                var de = (DomainEventMessage<FakeEvent>)detemp;

                de.AggregateId.Should().Be("aggid");
                de.AggregateName.Should().Be("aggname");
                de.DomainEventType.Should().Be("FakeEvent");

                de.DomainEvent.Id.Should().Be(ev.Id);
                de.DomainEvent.Text.Should().Be(ev.Text);
                de.DomainEvent.AggregateId.Should().Be(ev.AggregateId);
                de.DomainEvent.TimeStampUtc.Should().Be(ev.TimeStampUtc);
                de.DomainEvent.UserId.Should().Be(ev.UserId);
            }
        }
    }
}
