using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Domain;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.Messaging;

namespace ConsoleApp.Sagas
{
    public class MySaga : SagaBase2
        , IHandleMessage<FakeCommand>
    {
        public MySaga(ILoggerFactory loggerFactory, ISagaServices services) : base(loggerFactory, services)
        {
        }

        protected override async Task RegisterMessagesAsync()
        {
            await Bus.SubscribeCommand<FakeCommand>(HandleAsync, CancellationToken);
        }

        public Task HandleAsync(FakeCommand message)
        {
            Logger.LogInformation(message.Text);
            return Task.CompletedTask;
        }
    }
}
