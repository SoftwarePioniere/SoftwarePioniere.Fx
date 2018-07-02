using System;
using System.Threading.Tasks;
using FluentAssertions;
using Foundatio.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.DomainModel.FakeDomain;
using SoftwarePioniere.Messaging;
using Xunit.Abstractions;

namespace SoftwarePioniere.DomainModel
{
    public abstract class MessageBusTestsBase : TestBase
    {
        private IMessageBus CreateInstance()
        {
            var sagas = ServiceProvider.GetServices<Saga>();
            foreach (var saga in sagas)
            {
                _logger.LogInformation("Saga Started {SagaType}", saga.GetType());
            }

            var handlers = ServiceProvider.GetServices<IMessageHandler>();
            foreach (var handler in handlers)
            {
                _logger.LogInformation("Handler Started {HandlerType}", handler.GetType());
            }

            return GetService<IMessageBus>();
        }


        public virtual async Task RegisteredHandlerExecutionWillContinueOnErrorAsync()
        {
            ServiceCollection
                .AddSingleton<IMessageSubscriber>(c => c.GetService<IMessageBus>())
                .AddSingleton<IMessageHandler, FakeEventHandlerWithError>()
                .AddSingleton<IMessageHandler, FakeEventHandler>()
                ;

            var bus = CreateInstance();
            var @event = FakeEvent.Create();
            await bus.PublishAsync(@event);
            await Task.Delay(100);
            FakeEventHandler.HandledBy.Should().Contain(x => x == @event.Id);
        }


        public virtual async Task RegisteredEventHandlerWillBeCalledAsync()
        {
            ServiceCollection
                .AddSingleton<IMessageSubscriber>(c => c.GetService<IMessageBus>())
                .AddSingleton<IMessageHandler, FakeEventHandler>()
                ;

            var bus = CreateInstance();
            var @event = FakeEvent.Create();
            await bus.PublishAsync(@event);
            await Task.Delay(1000);
            FakeEventHandler.HandledBy.Should().Contain(x => x == @event.Id);
        }

        public virtual async Task SagaWillPublishCommandSucceededAsync()
        {
            ServiceCollection
                .AddSingleton<IMessageSubscriber>(c => c.GetService<IMessageBus>())
                .AddSingleton<IMessageHandler, FakeNotificationHandler>()
                .AddSingleton<Saga, FakeSaga>()
                ;

            var bus = CreateInstance();
            var cmd = new FakeCommand(Guid.NewGuid(), DateTime.UtcNow, "xxx", -1, "xxx", "Text1");
            await bus.PublishAsync(cmd);
            await Task.Delay(1000);
            FakeNotificationHandler.HandledByCommandSucceededNotification.Should().Contain(x => x == cmd.Id);
        }

        public virtual async Task SagaWillPublishCommandFailedAsync()
        {
            ServiceCollection
                .AddSingleton<IMessageSubscriber>(c => c.GetService<IMessageBus>())
                .AddSingleton<IMessageHandler, FakeNotificationHandler>()
                .AddSingleton<Saga, FakeSagaWithError>()
                ;

            var bus = CreateInstance();
            var cmd = new FakeCommand(Guid.NewGuid(), DateTime.UtcNow, "xxx", -1, "xxx", "Text1");
            await bus.PublishAsync(cmd);
            await Task.Delay(1000);
            FakeNotificationHandler.HandledByCommandFailedNotification.Should().Contain(x => x == cmd.Id);
        }

        protected MessageBusTestsBase(ITestOutputHelper output) : base(output)
        {
        }
    }
}
