using System.Threading.Tasks;
using Foundatio.Caching;
using Foundatio.Lock;
using Foundatio.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.Messaging;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.Tests.DomainModel.Services
{
    public class InMemoryMessageBusTests : MessageBusTestsBase
    {
        public InMemoryMessageBusTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection
                .AddSingleton<ISagaServices, SagaServices>()
                .AddSingleton<IMessageBusAdapter, DefaultMessageBusAdapter>()
                .AddSingleton<ISopiApplicationLifetime, SopiApplicationLifetime>()
                .AddSingleton<IRepository, Repository>()
                .AddSingleton<IEventStore, InMemoryEventStore>()
                .AddSingleton<ICacheClient, InMemoryCacheClient>()
                .AddSingleton<ILockProvider, CacheLockProvider>()
                .AddSingleton<IPersistentSubscriptionFactory, NullPersistentSubscriptionFactory>()
                .AddSingleton<IMessageBus>(new InMemoryMessageBus(new InMemoryMessageBusOptions()
                {
                    LoggerFactory = Log
                }));
        }

        [Fact]
        public override Task RegisteredEventHandlerWillBeCalledAsync()
        {
            return base.RegisteredEventHandlerWillBeCalledAsync();
        }

        [Fact]
        public override Task RegisteredHandlerExecutionWillContinueOnErrorAsync()
        {
            return base.RegisteredHandlerExecutionWillContinueOnErrorAsync();
        }

        [Fact]
        public override Task SagaWillPublishCommandSucceededAsync()
        {
            return base.SagaWillPublishCommandSucceededAsync();
        }

        [Fact]
        public override Task SagaWillPublishCommandFailedAsync()
        {
            return base.SagaWillPublishCommandFailedAsync();
        }
    }
}
