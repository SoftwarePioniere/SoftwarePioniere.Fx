using System;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.FakeDomain
{
    public sealed class FakeSagaWithError : SagaBase, IHandleMessage<FakeCommand>
    {
        public FakeSagaWithError(ILoggerFactory loggerFactory, IMessageBus bus) : base(loggerFactory, bus)
        {
           
        }

        public Task HandleAsync(FakeCommand message)
        {
            Logger.LogInformation("Handling FakeCommand - throwing Error");
            throw new InvalidOperationException();
        }

        public override void Initialize(CancellationToken cancellationToken = default)
        {
            SubscribeCommand<FakeCommand>(HandleAsync, cancellationToken);
        }
    }
}