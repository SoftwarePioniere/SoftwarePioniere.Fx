using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.FakeDomain
{
    public sealed class FakeSaga : SagaBase, IHandleMessage<FakeCommand>
    {
        public FakeSaga(ILoggerFactory loggerFactory, IMessageBus bus) : base(loggerFactory, bus)
        {
          
        }

        public Task HandleAsync(FakeCommand message)
        {
            Logger.LogInformation("Handling FakeCommand");
            return Task.CompletedTask;
        }

        public override void Initialize(CancellationToken cancellationToken = default(CancellationToken))
        {
            SubscribeCommand<FakeCommand>(HandleAsync, cancellationToken);
        }
    }
}
