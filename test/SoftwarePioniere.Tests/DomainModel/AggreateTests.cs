using FluentAssertions;
using SoftwarePioniere.Domain;
using SoftwarePioniere.DomainModel.FakeDomain;
using SoftwarePioniere.Messaging;
using Xunit;

namespace SoftwarePioniere.Tests.DomainModel
{
    public class AggreateTests
    {


        [Fact]
        public void NewEventsAreInUncommitedList()
        {
            var agg = FakeAggregate.Factory.Create();
            agg.DoFakeEvent("fake text");
            agg.GetUncommittedChanges().Should().NotBeEmpty();

        }

        [Fact]

        public void ClearsCommitedChanges()
        {
            var agg = FakeAggregate.Factory.Create();
            agg.DoFakeEvent("fake text");
            agg.GetUncommittedChanges().Should().NotBeEmpty();
            agg.MarkChangesAsCommitted();
            agg.GetUncommittedChanges().Should().BeEmpty();
        }

        [Fact]

        public void ApplyNewEvent()
        {
            var agg = FakeAggregate.Factory.Create();

            agg.DoFakeEvent("fake text");
            agg.Text.Should().Be("fake text");
        }

        [Fact]

        public void ApplyHistoryEvents()
        {
            var agg = new FakeAggregate();

            var @event = FakeEvent.Create();
            agg.SetId(@event.AggregateId);

            agg.LoadFromHistory(new[] { new EventDescriptor(@event, 1) });

            agg.Id.Should().Be(@event.AggregateId);
            agg.Text.Should().Be(@event.Text);
        }

        [Fact]
        public void ApplyChangeWithNoChangeShouldNotFail()
        {
            var agg = new FakeAggregate();
            agg.DoFakeEvent3();
            agg.Version.Should().Be(0);

        }      
    }
}
