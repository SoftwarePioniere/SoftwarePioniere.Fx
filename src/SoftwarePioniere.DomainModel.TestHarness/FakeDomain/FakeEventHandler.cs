using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.FakeDomain
{
    public class FakeEventHandler : IMessageHandler
        , IHandleMessage<FakeEvent>
        , IHandleMessage<FakeEvent2>
    {
        private readonly ILogger _logger;

        public FakeEventHandler(IMessageSubscriber subscriber, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            subscriber.SubscribeAsync<FakeEvent>(HandleAsync);
            subscriber.SubscribeAsync<FakeEvent2>(HandleAsync);
        }

        public static IList<Guid> HandledBy { get; } = new List<Guid>();

        public Task HandleAsync(FakeEvent message)
        {
            _logger.LogInformation("Handle FakeEvent");
            HandledBy.Add(message.Id);
            return Task.CompletedTask;
        }

        public Task HandleAsync(FakeEvent2 message)
        {
            _logger.LogInformation("Handle FakeEvent2 - throwing Error");
            throw new NotImplementedException();
        }
    }
}
