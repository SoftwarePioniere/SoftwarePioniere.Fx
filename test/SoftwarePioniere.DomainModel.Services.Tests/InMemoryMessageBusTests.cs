using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.DomainModel.Services.Tests
{
    public class InMemoryMessageBusTests : MessageBusTestsBase
    {
        public InMemoryMessageBusTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection.AddSingleton<IMessageBus>(new InMemoryMessageBus(new InMemoryMessageBusOptions()
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
