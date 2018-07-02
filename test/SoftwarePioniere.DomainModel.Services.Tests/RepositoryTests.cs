using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Foundatio.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SoftwarePioniere.DomainModel.Exceptions;
using SoftwarePioniere.DomainModel.FakeDomain;
using SoftwarePioniere.Messaging;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.DomainModel.Services.Tests
{
    public class RepositoryTests : TestBase, IRepositoryTests
    {
        private IRepository CreateInstance()
        {
            return GetService<IRepository>();
        }

        [Fact]
        public async Task SaveCallsEventStoreSavingAsync()
        {
            var mockStore = new Mock<IEventStore>();

            mockStore.Setup(x =>
                    x.SaveEventsAsync<FakeAggregate>(It.IsAny<string>(), It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<int>())
                )
                .Returns(Task.CompletedTask)
                .Verifiable();

            ServiceCollection.AddSingleton(mockStore.Object)
                .AddSingleton(Mock.Of<IMessagePublisher>());

            var repo = CreateInstance();

            var agg = FakeAggregate.Factory.Create();
            agg.DoFakeEvent("faketext");
            await repo.SaveAsync(agg, -1);

            mockStore.Verify();

        }

        [Fact]
        public async Task EventsWillBePushblishedAfterSavingAsync()
        {

            var mockPublisher = new Mock<IMessagePublisher>();

            mockPublisher.Setup(x => x.PublishAsync(
                    It.IsIn(typeof(FakeEvent), typeof(DomainEventMessage<FakeEvent>)),
                    //It.IsAny<Type>(),
                    It.IsAny<IMessage>(), null, default)
                )
                .Returns(Task.CompletedTask)
                .Verifiable();

            ServiceCollection
                .AddSingleton(Mock.Of<IEventStore>())
                .AddSingleton(mockPublisher.Object)
                ;

            var repo = CreateInstance();

            var agg = FakeAggregate.Factory.Create();
            agg.DoFakeEvent("faketext");
            await repo.SaveAsync(agg, -1);

            mockPublisher.Verify();
        }

        [Fact]
        public async Task LoadCreatesAggregateAsync()
        {
            var @event = FakeEvent.Create();
            IList<EventDescriptor> list = new List<EventDescriptor> { new EventDescriptor(@event, 0) };

            var mockStore = new Mock<IEventStore>();
            mockStore.Setup(x => x.GetEventsForAggregateAsync<FakeAggregate>(@event.AggregateId.ToString()))
                .ReturnsAsync(list);

            ServiceCollection
                .AddSingleton(mockStore.Object)
                .AddSingleton(Mock.Of<IMessagePublisher>())
                ;

            var repo = CreateInstance();


            var agg = await repo.GetByIdAsync<FakeAggregate>(@event.AggregateId);
            agg.Id.Should().Be(@event.AggregateId);
        }

        [Fact]
        public void LoadThrowsExceptionOnWrongVersionAsync()
        {
            var events = FakeEvent.CreateList(10).ToArray();
            var @event = events[0];

            IList<EventDescriptor> list = new List<EventDescriptor>();
            for (int i = 0; i < events.Length; i++)
            {
                list.Add(new EventDescriptor(events[i], i));
            }

            var mockStore = new Mock<IEventStore>();
            mockStore.Setup(x => x.GetEventsForAggregateAsync<FakeAggregate>(@event.AggregateId.ToString()))
                .ReturnsAsync(list);

            ServiceCollection
                .AddSingleton(mockStore.Object)
                .AddSingleton(Mock.Of<IMessagePublisher>())
                ;

            var repo = CreateInstance();

            Action act = () => repo.GetByIdAsync<FakeAggregate>(@event.AggregateId, 2).GetAwaiter().GetResult();
            act.Should().Throw<ConcurrencyException>();
        }

        public RepositoryTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection//.AddSingleton<IMessageBus>(new InMemoryMessageBus())
                .AddSingleton<IRepository, Repository>()
                
                ;
        }
    }
}
