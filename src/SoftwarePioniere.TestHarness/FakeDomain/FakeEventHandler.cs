using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.FakeDomain
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FakeEventHandler : IMessageHandler
        , IHandleMessage<FakeEvent>
        , IHandleMessage<FakeEvent2>
    {
        private readonly IMessageSubscriber _subscriber;
        private readonly ILogger _logger;

        public FakeEventHandler(IMessageSubscriber subscriber, ILoggerFactory loggerFactory)
        {
            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
            _logger = loggerFactory.CreateLogger(GetType());

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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _subscriber.SubscribeAsync<FakeEvent>(HandleAsync, cancellationToken).ConfigureAwait(false);
            await _subscriber.SubscribeAsync<FakeEvent2>(HandleAsync, cancellationToken).ConfigureAwait(false);
        }
    }
}
