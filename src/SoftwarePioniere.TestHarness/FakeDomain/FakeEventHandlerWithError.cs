using System;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.FakeDomain
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FakeEventHandlerWithError : IMessageHandler, IHandleMessage<FakeEvent>
    {
        private readonly IMessageSubscriber _subscriber;
        private readonly ILogger _logger;
        public FakeEventHandlerWithError(IMessageSubscriber subscriber, ILoggerFactory loggerFactory)
        {
            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
            _logger = loggerFactory.CreateLogger(GetType());

        }

        public Task HandleAsync(FakeEvent message)
        {
            _logger.LogInformation("Handle FakeEvent - throwing Error");
            throw new InvalidOperationException();

        }



        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _subscriber.SubscribeAsync<FakeEvent>(HandleAsync, cancellationToken).ConfigureAwait(false);
        }
    }
}