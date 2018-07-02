using System;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.FakeDomain
{
    public class FakeEventHandlerWithError : IMessageHandler, IHandleMessage<FakeEvent>
    {
        private readonly ILogger _logger;
        public FakeEventHandlerWithError(IMessageSubscriber subscriber, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            subscriber.SubscribeAsync<FakeEvent>(HandleAsync);
        }

        public Task HandleAsync(FakeEvent message)
        {
            _logger.LogInformation("Handle FakeEvent - throwing Error");
            throw new InvalidOperationException();

        }
    }
}