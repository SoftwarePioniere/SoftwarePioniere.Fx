using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Foundatio.AsyncEx;
using Foundatio.Caching;
using Foundatio.Lock;
using Foundatio.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Domain;
using SoftwarePioniere.EventStore.Domain;
using SoftwarePioniere.EventStore.Projections;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Projections;
using SoftwarePioniere.ReadModel;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.EventStore.Tests
{
    public class EventStoreProjectionContextTests : TestBase
    {
        public EventStoreProjectionContextTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection
                .AddEventStoreConnection(c => new TestConfiguration().ConfigurationRoot.Bind("EventStore", c))
                //   .AddEventStoreProjectionServices()
                .AddEventStoreDomainServices()
                .AddInMemoryEntityStore()
                .AddInMemoryCacheClient()
                .AddInMemoryMessageBus()

                .AddSingleton<ICacheAdapter, CacheAdapter>()
                .AddSingleton<ILockProvider>(pr =>
                    new CacheLockProvider(pr.GetRequiredService<ICacheClient>(),
                        pr.GetRequiredService<IMessageBus>(),
                        pr.GetRequiredService<ILoggerFactory>()
                    ))
                .AddSingleton<IMessageBusAdapter, DefaultMessageBusAdapter>()
                .AddSingleton<ISopiApplicationLifetime, SopiApplicationLifetime>()
                .AddProjectionServices(c => new TestConfiguration().ConfigurationRoot.Bind("Projection", c))
            ;

        }

        [Fact]
        public async Task TestWithoutQueue()
        {
            AggName = "SoftwarePionierTests" + Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower();

            var prov = GetService<EventStoreConnectionProvider>();
            await prov.InitializeAsync(CancellationToken.None);

            var es = GetService<DomainEventStore>();

            var proj = new TestProjector1(Log, GetService<IProjectorServices>());

            var ctx = new EventStoreProjectionContext(Log,
                GetService<EventStoreConnectionProvider>(),
                GetService<IEntityStore>(),
                proj, false,
                typeof(TestProjector1).FullName);

            long totalEvents = await AddEvents(es);

            var resetEvent = new AsyncManualResetEvent(false);
            ctx.LiveProcessingStartedAction = () => resetEvent.Set();

            await ctx.StartInitializationModeAsync();
            await ctx.StopInitializationModeAsync();

            await ctx.StartSubscription();

            await resetEvent.WaitAsync();

            ctx.IsLiveProcessing.Should().BeTrue();
            ctx.Status.Should().NotBeNull();
            ctx.Status.LastCheckPoint.Should().Be(totalEvents);

            await AddEvents(es);
            await AddEvents(es);
            await AddEvents(es);
        }

        private static async Task<long> AddEvents(DomainEventStore es)
        {
            long totalEvents = -1;

            for (int i = 0;
                i < 11;
                i++)
            {
                var aggId = Guid.NewGuid().ToString();

                var evliste = new List<IDomainEvent>();

                for (int j = 0;
                    j < 10;
                    j++)
                {
                    evliste.Add(
                        new FakeEvent(Guid.NewGuid(), DateTime.UtcNow, "a", aggId, $"Text {j}")
                    );

                    evliste.Add(
                        new FakeEvent2(Guid.NewGuid(), DateTime.UtcNow, "b", aggId, $"Text 2 {j}")
                    );

                    evliste.Add(
                        new FakeEvent3(Guid.NewGuid(), DateTime.UtcNow, "b", aggId)
                    );
                }

                totalEvents += evliste.Count;

                var streamName = AggregateIdToStreamName(aggId);
                await es.SaveEventsAsync<TestAggregate1>(aggId,
                    evliste,
                    AggregateRoot.StartVersion + evliste.Count,
                    streamName);
            }

            return totalEvents;
        }


        [Fact]
        public async Task TestWithQueue()
        {
            AggName = "SoftwarePionierTests" + Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower();

            var prov = GetService<EventStoreConnectionProvider>();
            await prov.InitializeAsync(CancellationToken.None);

            var es = GetService<DomainEventStore>();

            var proj = new TestProjector1(Log, GetService<IProjectorServices>());

            var ctx = new EventStoreProjectionContext(Log,
                GetService<EventStoreConnectionProvider>(),
                GetService<IEntityStore>(),
                proj, true,
                typeof(TestProjector1).FullName);

            long totalEvents = await AddEvents(es);


            await ctx.StartInitializationModeAsync();
            await ctx.StopInitializationModeAsync();

            await ctx.StartSubscription();

            await Task.Delay(TimeSpan.FromSeconds(5));

            ctx.IsLiveProcessing.Should().BeTrue();
            ctx.Status.Should().NotBeNull();
            ctx.Status.LastCheckPoint.Should().Be(totalEvents);
        }


        //[Fact]
        //public async Task Test2()
        //{
        //    await Test1();


        //}

        public static string AggregateIdToStreamName(string id)
        {
            var aggName = AggName;
            return $"{aggName}-{id.ToUpper().Replace("-", "")}";
        }

        public static string AggName { get; set; }


        public class TestAggregate1 : AggregateRoot
        {

        }

        public class TestProjector1 : ReadModelProjectorBase5<FakeEntity>
        {
            public TestProjector1(ILoggerFactory loggerFactory, IProjectorServices services) : base(loggerFactory, services)
            {
                StreamName = "$ce-" + AggName;
            }

            public override async Task ProcessEventAsync(IDomainEvent domainEvent)
            {
                try
                {
                    var b = await HandleIfAsync<FakeEvent>(HandleAsync, domainEvent);
                    if (!b)
                    {
                        b = await HandleIfAsync<FakeEvent2>(HandleAsync, domainEvent);
                        if (!b)
                        {
                            await HandleIfAsync<FakeEvent3>(HandleAsync, domainEvent);
                        }
                    }

                }
                catch (Exception ex) when (LogError(ex))
                {
                    Logger.LogError(ex, "Error on ProcessEventAsync {@DomainEvent}", domainEvent);
                }
            }

            protected bool LogError(Exception ex)
            {
                Logger.LogError(ex, ex.GetBaseException().Message);
                return true;
            }

            private Task HandleAsync(FakeEvent3 arg)
            {
                return LoadAndSaveEveryTimeAsync(arg,
                    entity =>
                    {
                        entity.GuidValue = arg.Id;
                        entity.Dict1 = entity.Dict1.EnsureDictContainsValue(arg.UserId, arg.AggregateId);
                    });
            }

            private Task HandleAsync(FakeEvent2 arg)
            {
                return LoadAndSaveEveryTimeAsync(arg,
                    entity =>
                    {
                        entity.StringValue = arg.Text;
                        entity.Dict1 = entity.Dict1.EnsureDictContainsValue(arg.UserId, arg.AggregateId);
                    });
            }

            private Task HandleAsync(FakeEvent arg)
            {
                return LoadAndSaveEveryTimeAsync(arg,
                    entity =>
                    {
                        entity.StringValue = arg.Text;
                        entity.Dict1 = entity.Dict1.EnsureDictContainsValue(arg.UserId, arg.AggregateId);
                    });
            }

            protected override object CreateIdentifierItem(FakeEntity entity)
            {
                return entity;
            }

            protected override async Task<EntityDescriptor<FakeEntity>> LoadItemAsync(IMessage message)
            {
                var item = new EntityDescriptor<FakeEntity>
                {
                    Entity = null
                };

                if (message is IFakeAggregateIdEvent x)
                {
                    item = await LoadAsync(x.AggregateId);
                    item.Id = x.AggregateId;
                }

                return item;
            }
        }
    }
}
