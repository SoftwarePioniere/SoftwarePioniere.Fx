using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.DomainModel;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Sample.ConsoleApp.Sagas
{
    public class MySaga : SagaBase2
        , IHandleWithState<FakeCommand>
    {
        public MySaga(ILoggerFactory loggerFactory, ISagaServices services) : base(loggerFactory, services)
        {
        }

        protected override async Task RegisterMessagesAsync()
        {
            await Bus.SubscribeCommand<FakeCommand>(HandleAsync, CancellationToken);
        }

        public Task HandleAsync(FakeCommand message, IDictionary<string, string> state)
        {
            Logger.LogInformation(message.Text);
            return Task.CompletedTask;
        }
    }
}
