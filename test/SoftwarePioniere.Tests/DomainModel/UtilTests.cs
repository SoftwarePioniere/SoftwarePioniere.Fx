using FluentAssertions;
using SoftwarePioniere.DomainModel.FakeDomain;
using SoftwarePioniere.Messaging;
using Xunit;

namespace SoftwarePioniere.Tests.DomainModel
{
    public class UtilTests
    {


        [Fact]
        public void AggregateIdToStreamNameWithAttributeTest()
        {
            var name = Domain.Util.AggregateIdToStreamName(typeof(FakeAggregate), "id1");
            name.Should().Be(string.Concat(Constants.BoundedContextName, "_Fake-ID1"));

            var name2 = Domain.Util.AggregateIdToStreamName<FakeAggregate>("id1");
            name2.Should().Be(string.Concat(Constants.BoundedContextName, "_Fake-ID1"));
        }

        [Fact]
        public void AggregateIdToStreamNameWithoutAttributeTest()
        {
            var name = Domain.Util.AggregateIdToStreamName(typeof(FakeAggregateWithoutNameAttribute), "id1");
            name.Should().Be("FakeAggregateWithoutNameAttribute-ID1");
        }

        [Fact]
        public void GetAggregateNameWithAttributeTest()
        {
            // ReSharper disable once InvokeAsExtensionMethod
            var name = Domain.Util.GetAggregateName(typeof(FakeAggregate));
            name.Should().Be(string.Concat(Constants.BoundedContextName, "_Fake"));
        }

        [Fact]
        public void GetAggregateNameWithoutAttributeTest()
        {
            // ReSharper disable once InvokeAsExtensionMethod
            var name = Domain.Util.GetAggregateName(typeof(FakeAggregateWithoutNameAttribute));
            name.Should().Be("FakeAggregateWithoutNameAttribute");
        }

        [Fact]
        public void CanCreateDomainEventMessages()
        {
            var ev = FakeEvent.Create();
            var agg = FakeAggregate.Factory.Create("1");

            {
                var de = ev.CreateAggregateDomainEventMessage(agg);

                de.AggregateId.Should().Be(agg.Id);
                de.AggregateType.Should().Be(agg.GetType().GetTypeShortName());
                de.DomainEventType.Should().Be("SoftwarePioniere.Messaging.FakeEvent, SoftwarePioniere.TestHarness");

                var pev = de.GetEvent<FakeEvent>();
                pev.Id.Should().Be(ev.Id);
                pev.Text.Should().Be(ev.Text);
                pev.AggregateId.Should().Be(ev.AggregateId);
                pev.TimeStampUtc.Should().Be(ev.TimeStampUtc);
                pev.UserId.Should().Be(ev.UserId);
            }

            //{
            //    var de = (AggregateDomainEventMessage<FakeAggregate, FakeEvent>)ev.CreateTypedAggregateDomainEventMessage(agg);
            //    de.Should().NotBeNull();

            //    de.AggregateId.Should().Be(agg.Id);
            //    de.AggregateType.Should().Be(agg.GetType().GetTypeShortName());
            //    de.DomainEventType.Should().Be("SoftwarePioniere.Messaging.FakeEvent, SoftwarePioniere.Messaging.TestHarness");

            //    de.DomainEventContent.Id.Should().Be(ev.Id);
            //    de.DomainEventContent.Text.Should().Be(ev.Text);
            //    de.DomainEventContent.AggregateId.Should().Be(ev.AggregateId);
            //    de.DomainEventContent.TimeStampUtc.Should().Be(ev.TimeStampUtc);
            //    de.DomainEventContent.UserId.Should().Be(ev.UserId);
            //}
        }


    }
}
