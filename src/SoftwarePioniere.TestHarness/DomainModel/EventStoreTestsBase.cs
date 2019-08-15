﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Domain.Exceptions;
using SoftwarePioniere.DomainModel.FakeDomain;
using SoftwarePioniere.Messaging;
using Xunit.Abstractions;

namespace SoftwarePioniere.DomainModel
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
            var exists = await store.CheckAggregateExists<FakeAggregate>(@event.AggregateId);
            exists.Should().BeFalse();

            await store.SaveEventsAsync<FakeAggregate>(@event.AggregateId, new IDomainEvent[] { @event }, 0);
            exists = await store.CheckAggregateExists<FakeAggregate>(@event.AggregateId);
            exists.Should().BeTrue();
        }

        public virtual async Task SaveAndLoadContainsAllEventsForAnAggregate()
        {
            _logger.LogInformation("Running SaveAndLoadContainsAllEventsForAnAggregate");
            await Task.Delay(0);

            var store = CreateInstance();

            {
                var @event = FakeEvent.Create();
                store.SaveEventsAsync<FakeAggregate>(@event.AggregateId, new IDomainEvent[] { @event }, 0).Wait();

                var loaded = store.GetEventsForAggregateAsync<FakeAggregate>(@event.AggregateId).Result;
                loaded.Should().Contain(x => x.EventData.Id == @event.Id);
            }


            {
                var save = FakeEvent.CreateList(155).ToArray();
                store.SaveEventsAsync<FakeAggregate>(save.First().AggregateId, save, 154).Wait();

                var loaded = store.GetEventsForAggregateAsync<FakeAggregate>(save.First().AggregateId).Result;
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

            Action act = () => store.GetEventsForAggregateAsync<FakeAggregate>(Guid.NewGuid().ToString()).Wait();
            act.Should().Throw<AggregateNotFoundException>();
        }

        public virtual async Task SaveThrowsErrorIfVersionsNotMatch()
        {
            _logger.LogInformation("Running SaveThrowsErrorIfVersionsNotMatch");

            var store = CreateInstance();

            await Task.Delay(0);

            var event1 = FakeEvent.Create();
            store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { event1 }, 0).Wait();

            var event2 = FakeEvent.Create();

            Action act = () => store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { event2 }, 42).Wait();
            act.Should().Throw<AggregateException>().WithInnerException<ConcurrencyException>();

        }

        public virtual async Task SavesEventsWithExpectedVersion()
        {
            _logger.LogInformation("Running SavesEventsWithExpectedVersion");


            var store = CreateInstance();
            await Task.Delay(0);

            {
                var agg = FakeAggregate.Factory.Create();
                for (int i = 0; i < 133; i++)
                {
                    agg.DoFakeEvent($"schleife {i}");
                }
                store.SaveEventsAsync<FakeAggregate>(agg.Id, agg.GetUncommittedChanges(), agg.Version).Wait();
                agg.MarkChangesAsCommitted();

                agg.DoFakeEvent2("fake 2");
                store.SaveEventsAsync<FakeAggregate>(agg.Id, agg.GetUncommittedChanges(), agg.Version).Wait();
                agg.MarkChangesAsCommitted();

                var events = store.GetEventsForAggregateAsync<FakeAggregate>(agg.Id).Result;
                var lastEvent = events.Last();

                agg.Version.Should().Be(lastEvent.Version);


                agg.DoFakeEvent2("fake 3");
                Action act = () => store.SaveEventsAsync<FakeAggregate>(agg.Id, agg.GetUncommittedChanges(), agg.Version + 99).Wait();
                act.Should().Throw<AggregateException>().WithInnerException<ConcurrencyException>();


            }

            {
                var event1 = FakeEvent.Create();
                store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { event1 }, 0).Wait();
                store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { FakeEvent.Create() }, 1).Wait();
                store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { FakeEvent.Create() }, 2).Wait();
                store.SaveEventsAsync<FakeAggregate>(event1.AggregateId, new IDomainEvent[] { FakeEvent.Create(), FakeEvent.Create() }, 4).Wait();
            }


        }
    }
}
