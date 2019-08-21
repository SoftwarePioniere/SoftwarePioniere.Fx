using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Foundatio.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Domain;
using SoftwarePioniere.DomainModel.FakeDomain;
using SoftwarePioniere.Messaging;
using Xunit.Abstractions;

namespace SoftwarePioniere.DomainModel
{
    public abstract class MessageBusTestsBase : TestBase
    {
        private async Task<IMessageBus> CreateInstance()
        {
            var sagas = ServiceProvider.GetServices<ISaga>();
            foreach (var saga in sagas)
            {
                _logger.LogInformation("Saga Initialize {SagaType}", saga.GetType());
                await saga.StartAsync(CancellationToken.None);
            }

            var handlers = ServiceProvider.GetServices<IMessageHandler>();
            foreach (var handler in handlers)
            {
                _logger.LogInformation("Handler Initialize {HandlerType}", handler.GetType());
                await handler.StartAsync(CancellationToken.None);
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

            var bus = await CreateInstance();
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


            var bus = await CreateInstance();
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
                .AddSingleton<ISaga, FakeSaga>()
                ;

            var bus = await CreateInstance();
            var cmd = new FakeCommand(Guid.NewGuid(), DateTime.UtcNow, "xxx", -1, Guid.NewGuid().ToString(), "Text1");
            await bus.PublishAsync(cmd);
            await Task.Delay(1000);
            FakeNotificationHandler.HandledByCommandSucceededNotification.Should().Contain(x => x == cmd.Id);
        }

        public virtual async Task SagaWillPublishCommandFailedAsync()
        {
            ServiceCollection
                .AddSingleton<IMessageSubscriber>(c => c.GetService<IMessageBus>())
                .AddSingleton<IMessageHandler, FakeNotificationHandler>()
                .AddSingleton<ISaga, FakeSagaWithError>()
                ;

            var bus = await CreateInstance();
            var cmd = new FakeCommand(Guid.NewGuid(), DateTime.UtcNow, "xxx", -1, Guid.NewGuid().ToString(), "Text1");
            await bus.PublishAsync(cmd);
            await Task.Delay(1000);
            FakeNotificationHandler.HandledByCommandFailedNotification.Should().Contain(x => x == cmd.Id);
        }

        protected MessageBusTestsBase(ITestOutputHelper output) : base(output)
        {
        }
    }
}
