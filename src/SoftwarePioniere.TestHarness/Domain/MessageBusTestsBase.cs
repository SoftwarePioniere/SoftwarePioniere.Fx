using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Foundatio.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.Messaging;
using Xunit.Abstractions;

namespace SoftwarePioniere.Domain
{
    public abstract class MessageBusTestsBase : TestBase
    {
        private async Task<IMessageBus> CreateInstance()
        {
            var sagas = ServiceProvider.GetServices<ISaga>();
            foreach (var saga in sagas)
            {
                _logger.LogInformation("Saga Initialize {SagaType}", saga.GetType());
                await saga.StartAsync(CancellationToken.None).ConfigureAwait(false);
            }

            var handlers = ServiceProvider.GetServices<IMessageHandler>();
            foreach (var handler in handlers)
            {
                _logger.LogInformation("Handler Initialize {HandlerType}", handler.GetType());
                await handler.StartAsync(CancellationToken.None).ConfigureAwait(false);
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

            var bus = await CreateInstance().ConfigureAwait(false);
            var @event = FakeEvent.Create();
            await bus.PublishAsync(@event).ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
            FakeEventHandler.HandledBy.Should().Contain(x => x == @event.Id);
        }


        public virtual async Task RegisteredEventHandlerWillBeCalledAsync()
        {
            ServiceCollection
                .AddSingleton<IMessageSubscriber>(c => c.GetService<IMessageBus>())
                .AddSingleton<IMessageHandler, FakeEventHandler>()
                ;


            var bus = await CreateInstance().ConfigureAwait(false);
            var @event = FakeEvent.Create();
            await bus.PublishAsync(@event).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            FakeEventHandler.HandledBy.Should().Contain(x => x == @event.Id);
        }

        public virtual async Task SagaWillPublishCommandSucceededAsync()
        {
            ServiceCollection
                .AddSingleton<IMessageSubscriber>(c => c.GetService<IMessageBus>())
                .AddSingleton<IMessageHandler, FakeNotificationHandler>()
                .AddSingleton<ISaga, FakeSaga>()
                ;

            var bus = await CreateInstance().ConfigureAwait(false);
            var cmd = new FakeCommand(Guid.NewGuid(), DateTime.UtcNow, "xxx", -1, Guid.NewGuid().ToString(), "Text1");
            await bus.PublishAsync(cmd).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            FakeNotificationHandler.HandledByCommandSucceededNotification.Should().Contain(x => x == cmd.Id);
        }

        public virtual async Task SagaWillPublishCommandFailedAsync()
        {
            ServiceCollection
                .AddSingleton<IMessageSubscriber>(c => c.GetService<IMessageBus>())
                .AddSingleton<IMessageHandler, FakeNotificationHandler>()
                .AddSingleton<ISaga, FakeSagaWithError>()
                ;

            var bus = await CreateInstance().ConfigureAwait(false);
            var cmd = new FakeCommand(Guid.NewGuid(), DateTime.UtcNow, "xxx", -1, Guid.NewGuid().ToString(), "Text1");
            await bus.PublishAsync(cmd).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            FakeNotificationHandler.HandledByCommandFailedNotification.Should().Contain(x => x == cmd.Id);
        }

        protected MessageBusTestsBase(ITestOutputHelper output) : base(output)
        {
        }
    }
}
