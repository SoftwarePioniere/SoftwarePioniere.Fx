using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.FakeDomain
{
    public sealed class FakeSaga : SagaBase2, IHandleMessage<FakeCommand>
    {
      
        public Task HandleAsync(FakeCommand message)
        {
            Logger.LogInformation("Handling FakeCommand");
            return Task.CompletedTask;
        }

        protected override async Task RegisterMessagesAsync()
        {
            await Bus.SubscribeCommand<FakeCommand>(HandleAsync);
        }

        public FakeSaga(ILoggerFactory loggerFactory, ISagaServices services) : base(loggerFactory, services)
        {
        }
    }
}
