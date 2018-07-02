using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.DomainModel.Services;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.FakeDomain
{
    public sealed class FakeSaga : SagaBase, IHandleMessage<FakeCommand>
    {
        public FakeSaga(ILoggerFactory loggerFactory, IMessageBus bus) : base(loggerFactory, bus)
        {
            SubscribeCommand<FakeCommand>(HandleAsync);
        }

        public Task HandleAsync(FakeCommand message)
        {
            Logger.LogInformation("Handling FakeCommand");
            return Task.CompletedTask;
        }
    }
}
