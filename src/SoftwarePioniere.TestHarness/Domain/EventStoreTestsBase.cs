﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Domain.Exceptions;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.Messaging;
using Xunit.Abstractions;

namespace SoftwarePioniere.Domain
{
    public abstract class EventStoreTestsBase : TestBase, IEventStoreTests
    {
        protected EventStoreTestsBase(ITestOutputHelper output) : base(output)
        {

        }

        protected IEventStore CreateInstance()
        {
            return GetService<IEventStore>();
        }

        public virtual async Task CheckAggregateExists()
        {
            _logger.LogInformation("Running CheckAggregateExists");
            var @event = FakeEvent.Create();

            var store = CreateInstance();
            var exists = await store.CheckAggregateExists<FakeAggregate>(@event.AggregateId).ConfigureAwait(false);
            exists.Should().BeFalse();

            await store.SaveEventsAsync<FakeAggregate>(@event.AggregateId, new IDomainEvent[] { @event }, 0).ConfigureAwait(false);
            exists = await store.CheckAggregateExists<FakeAggregate>(@event.AggregateId).ConfigureAwait(false);
            exists.Should().BeTrue();
        }

        public virtual async Task SaveAndLoadContainsAllEventsForAnAggregate()
        {
            _logger.LogInformation("Running SaveAndLoadContainsAllEventsForAnAggregate");
            await Task.Delay(0).ConfigureAwait(false);

            var store = CreateInstance();

            {
                var @event = FakeEvent.Create();
                await store.SaveEventsAsync<FakeAggregate>(@event.AggregateId, new IDomainEvent[] { @event }, 0).ConfigureAwait(false);

                var loaded = await store.GetEventsForAggregateAsync<FakeAggregate>(@event.AggregateId).ConfigureAwait(false);
                loaded.Should().Contain(x => x.EventData.Id == @event.Id);
            }


            {
                var save = FakeEvent.CreateList(155).ToArray();
                await store.SaveEventsAsync<FakeAggregate>(save.First().AggregateId, save, 154).ConfigureAwait(false);

                var loaded = await store.GetEventsForAggregateAsync<FakeAggregate>(save.First().AggregateId).ConfigureAwait(false);
                foreach (var savedEvent in save)
                {
                    loaded.Should().Contain(x => x.EventData.Id == savedEvent.Id);
                }

            }
        }

        public virtual void LoadThrowsErrorIfAggregateWithIdNotFound()
        {
            _logger.LogInformation("Running LoadThrowsErrorIfAggregateWithIdNotFound");


            var store = CreateInstance();

            Func<Task> f = async () => { await store.GetEventsForAggregateAsync<FakeAggregate>(Guid.NewGuid().ToString()).ConfigureAwait(false); };

            f.Should().Throw<AggregateNotFoundException>();
        }

        public virtual async Task SaveThrowsErrorIfVersionsNotMatch()
        {
            _logger.LogInformation("Running SaveThrowsErrorIfVersionsNotMatch");

            var store = CreateInstance();

            await Task.Delay(0).ConfigureAwait(false);

            var event1 = FakeEvent.Create();
            await store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { event1 }, 0).ConfigureAwait(false);

            var event2 = FakeEvent.Create();

            Func<Task> f = async () => { await store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { event2 }, 42).ConfigureAwait(false); };

            f.Should().Throw<ConcurrencyException>(); //.WithInnerException<ConcurrencyException>();

        }

        public virtual async Task SavesEventsWithExpectedVersion()
        {
            _logger.LogInformation("Running SavesEventsWithExpectedVersion");


            var store = CreateInstance();
            await Task.Delay(0).ConfigureAwait(false);

            {
                var agg = FakeAggregate.Factory.Create();
                for (int i = 0; i < 133; i++)
                {
                    agg.DoFakeEvent($"schleife {i}");
                }
                await store.SaveEventsAsync<FakeAggregate>(agg.AggregateId, agg.GetUncommittedChanges(), agg.Version).ConfigureAwait(false);
                agg.MarkChangesAsCommitted();

                agg.DoFakeEvent2("fake 2");
                await store.SaveEventsAsync<FakeAggregate>(agg.AggregateId, agg.GetUncommittedChanges(), agg.Version).ConfigureAwait(false);
                agg.MarkChangesAsCommitted();

                var events = await store.GetEventsForAggregateAsync<FakeAggregate>(agg.AggregateId).ConfigureAwait(false);
                var lastEvent = events.Last();

                agg.Version.Should().Be(lastEvent.Version);


                agg.DoFakeEvent2("fake 3");

                Func<Task> f = async () =>
                {
                    await store.SaveEventsAsync<FakeAggregate>(agg.AggregateId,
                        agg.GetUncommittedChanges(),
                        agg.Version + 99).ConfigureAwait(false);
                };

                f.Should().Throw<ConcurrencyException>();


            }

            {
                var event1 = FakeEvent.Create();
                await store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { event1 }, 0).ConfigureAwait(false);
                await store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { FakeEvent.Create() }, 1).ConfigureAwait(false);
                await store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { FakeEvent.Create() }, 2).ConfigureAwait(false);
                await store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { FakeEvent.Create(), FakeEvent.Create() }, 4).ConfigureAwait(false);
            }


        }
    }
}
