using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Domain;
using SoftwarePioniere.DomainModel;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.Tests.DomainModel.Services
{
    public class InMemoryEventStoreTests : EventStoreTestsBase
    {
        public InMemoryEventStoreTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection.AddSingleton<IEventStore, InMemoryEventStore>();
        }

        [Fact]
        public override Task CheckAggregateExists()
        {
            return base.CheckAggregateExists();
        }

        [Fact]
        public override void LoadThrowsErrorIfAggregateWithIdNotFound()
        {
            base.LoadThrowsErrorIfAggregateWithIdNotFound();
        }

        [Fact]
        public override Task SaveAndLoadContainsAllEventsForAnAggregate()
        {
            return base.SaveAndLoadContainsAllEventsForAnAggregate();
        }


        [Fact]
        public override Task SaveThrowsErrorIfVersionsNotMatch()
        {
            return base.SaveThrowsErrorIfVersionsNotMatch();
        }

        [Fact]
        public override Task SavesEventsWithExpectedVersion()
        {
            return base.SavesEventsWithExpectedVersion();
        }
    }
}
