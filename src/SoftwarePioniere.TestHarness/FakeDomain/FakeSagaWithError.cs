using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.FakeDomain
{
    public sealed class FakeSagaWithError : SagaBase2, IHandleMessage<FakeCommand>
    {


        public Task HandleAsync(FakeCommand message)
        {
            Logger.LogInformation("Handling FakeCommand - throwing Error");
            throw new InvalidOperationException();
        }


        protected override async Task RegisterMessagesAsync()
        {
            await Bus.SubscribeCommand<FakeCommand>(HandleAsync);
        }


        public FakeSagaWithError(ILoggerFactory loggerFactory, ISagaServices services) : base(loggerFactory, services)
        {
        }
    }
}