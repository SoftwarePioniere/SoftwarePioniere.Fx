using System;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.DomainModel.Services;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.FakeDomain
{
    public sealed class FakeSagaWithError : SagaBase, IHandleMessage<FakeCommand>
    {
        public FakeSagaWithError(ILoggerFactory loggerFactory, IMessageBus bus) : base(loggerFactory, bus)
        {
            SubscribeCommand<FakeCommand>(HandleAsync);
        }

        public Task HandleAsync(FakeCommand message)
        {
            Logger.LogInformation("Handling FakeCommand - throwing Error");
            throw new InvalidOperationException();
        }
    }
}