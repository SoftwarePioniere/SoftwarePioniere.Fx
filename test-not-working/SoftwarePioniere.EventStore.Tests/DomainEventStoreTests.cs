using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Domain;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.EventStore.Tests
{
    public class DomainEventStoreTests : EventStoreTestsBase
    {
        public DomainEventStoreTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection
                .AddEventStoreTestConfig(_logger)
                ;


            ServiceCollection.AddEventStoreDomainServices();
        }


        private Task InitAsync()
        {
            var prov = GetService<EventStoreConnectionProvider>();
            return prov.InitializeAsync(CancellationToken.None);
        }


        [Fact]
        public override async Task CheckAggregateExists()
        {
            await InitAsync();
            await base.CheckAggregateExists();
        }

        [Fact]
        public override async void LoadThrowsErrorIfAggregateWithIdNotFound()
        {
            await InitAsync();
            base.LoadThrowsErrorIfAggregateWithIdNotFound();
        }

        [Fact]
        public override async Task SaveAndLoadContainsAllEventsForAnAggregate()
        {
            await InitAsync();
            await base.SaveAndLoadContainsAllEventsForAnAggregate();
        }


        [Fact]
        public override async Task SaveThrowsErrorIfVersionsNotMatch()
        {
            await InitAsync();
            await base.SaveThrowsErrorIfVersionsNotMatch();
        }

        [Fact]
        public override async Task SavesEventsWithExpectedVersion()
        {
            await InitAsync();
            await base.SavesEventsWithExpectedVersion();
        }
    }
}
