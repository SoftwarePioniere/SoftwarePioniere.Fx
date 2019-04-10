using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.DomainModel;

namespace SoftwarePioniere.Messaging
{
    public class DefaultMessageBusAdapter : IMessageBusAdapter
    {
        private readonly IMessageBus2 _bus;
        private readonly ITelemetryAdapter _telemetryAdapter;
   
        private readonly ILogger _logger;

        public DefaultMessageBusAdapter(ILoggerFactory loggerFactory, IMessageBus2 bus, ITelemetryAdapter telemetryAdapter)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            _logger = loggerFactory.CreateLogger(GetType());

            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _telemetryAdapter = telemetryAdapter ?? throw new ArgumentNullException(nameof(telemetryAdapter));        
        }

        private bool LogError(Exception ex)
        {
            _logger.LogError(ex, "Ein Fehler ist aufgetreten {Message}", ex.GetBaseException().Message);
            return true;
        }

        public Task PublishAsync(Type messageType, object message, TimeSpan? delay = null,
            CancellationToken cancellationToken = new CancellationToken(), IDictionary<string, string> parentState = null)
        {
            if (parentState == null)
                parentState = new Dictionary<string, string>();

            var operationName = $"PUBLISH {messageType.GetTypeShortName()}";

            return _telemetryAdapter.RunDependencyAsync(operationName,
                "BUS",
                async state =>
                {
                    if (!typeof(IMessageWrapper).IsAssignableFrom(messageType) &&
                        typeof(IMessage).IsAssignableFrom(messageType))
                    {
                        var imessage = (IMessage)message;
                        var created = imessage.CreateMessageWrapper(state);
                        await _bus.PublishAsync(created.GetType(), created, delay, cancellationToken);
                    }
                    else
                    {
                        await _bus.PublishAsync(messageType, message, delay, cancellationToken);
                    }
                },
                parentState,
                _logger);
        }

        public Task PublishAsync<T>(T message, TimeSpan? delay = null, CancellationToken cancellationToken = new CancellationToken(),
            IDictionary<string, string> parentState = null) where T : class, IMessage
        {
            if (parentState == null)
                parentState = new Dictionary<string, string>();

            var messageType = typeof(T);

            var operationName = $"PUBLISH {messageType.GetTypeShortName()}";

            return _telemetryAdapter.RunDependencyAsync(operationName,
                "BUS",
                async state =>
                {
                    if (!typeof(IMessageWrapper).IsAssignableFrom(messageType) &&
                        typeof(IMessage).IsAssignableFrom(messageType))
                    {
                        var imessage = (IMessage)message;
                        var created = imessage.CreateMessageWrapper(state);
                        await _bus.PublishAsync(created.GetType(), created, delay, cancellationToken);
                    }
                    else
                    {
                        await _bus.PublishAsync(messageType, message, delay, cancellationToken);
                    }
                },
                parentState,
                _logger);
        }

        public async Task SubscribeMessage<T>(Func<T, IDictionary<string, string>, Task> handler, CancellationToken cancellationToken = new CancellationToken()) where T : class, IMessage
        {
            _logger.LogDebug("Subscribing to Message {MessageType}", typeof(T).GetTypeShortName());
            var bus = _bus;

            await bus.SubscribeAsync<MessageWrapper>(async (wrappedMessage, token) =>
            {
                if (!wrappedMessage.IsWrappedType<T>())
                    return;

                var message = wrappedMessage.GetWrappedMessage<T>();
                var state = wrappedMessage.Properties ?? new Dictionary<string, string>();

                await handler(message, state.Copy());
            },
            wrapper =>
            {
                if (!wrapper.IsWrappedType<T>())
                    return false;
                return true;
            },
            cancellationToken);
        }

        public async Task SubscribeCommand<T>(Func<T, IDictionary<string, string>, Task> handler, CancellationToken cancellationToken = new CancellationToken()) where T : class, ICommand
        {
            _logger.LogDebug("Subscribing to Command {CommandType}", typeof(T).GetTypeShortName());
            var bus = _bus;
            await bus.SubscribeAsync<MessageWrapper>(async (wrappedMessage, token) =>
                {
                    if (!wrappedMessage.IsWrappedType<T>())
                        return;

                    var message = wrappedMessage.GetWrappedMessage<T>();
                    var state = wrappedMessage.Properties ?? new Dictionary<string, string>();

                    await handler(message, state.Copy());
                },
                wrapper =>
                {
                    if (!wrapper.IsWrappedType<T>())
                        return false;
                    return true;
                },
                cancellationToken);
        }

        public async Task SubscribeAggregateDomainEvent<TAggregate, TDomainEvent>(Func<TDomainEvent, AggregateTypeInfo<TAggregate>, IDictionary<string, string>, Task> handler,
            CancellationToken cancellationToken = new CancellationToken()) where TAggregate : IAggregateRoot where TDomainEvent : class, IDomainEvent
        {
            _logger.LogDebug("Subscribing to AggregateEvent {AggregateName} {MessageType}",
                typeof(TAggregate).GetAggregateName(),
                typeof(TDomainEvent).GetTypeShortName());

            var bus = _bus; // await GetSubscriber();

            await bus.SubscribeAsync<MessageWrapper>(
                async (wrappedMessage, token) =>
                {
                    if (!wrappedMessage.IsWrappedType<AggregateDomainEventMessage>())
                        return;

                    var message = wrappedMessage.GetWrappedMessage<AggregateDomainEventMessage>();

                    if (!message.IsAggregate<TAggregate>() || !message.IsEventType<TDomainEvent>())
                        return;

                    var domainEvent = message.GetEvent<TDomainEvent>();

                    var eventType = message.DomainEventType;
                    var aggregateType = message.AggregateType;
                    var eventId = domainEvent.Id.ToString();
                    var aggregateName = typeof(TAggregate).GetAggregateName();
                    var aggregateId = message.AggregateId;
                    var eventName = typeof(TDomainEvent).Name;

                    var state = wrappedMessage.Properties ?? new Dictionary<string, string>();
                    state.AddProperty("EventType", eventType)
                        .AddProperty("EventId", eventId)
                        .AddProperty("EventName", eventName)
                        .AddProperty("AggregateType", aggregateType)
                        .AddProperty("AggregateName", aggregateName)
                        .AddProperty("AggregateId", aggregateId)
                        ;

                    await handler(domainEvent,
                        new AggregateTypeInfo<TAggregate>(message.AggregateId),
                        state.Copy());
                },
                wrapper =>
                {
                    if (!wrapper.IsWrappedType<AggregateDomainEventMessage>())
                        return false;

                    var message = wrapper.GetWrappedMessage<AggregateDomainEventMessage>();

                    if (!message.IsAggregate<TAggregate>() || !message.IsEventType<TDomainEvent>())
                        return false;

                    return true;
                }, cancellationToken);
        }

        public async Task<MessageResponse> PublishCommandAsync<T>(T cmd, CancellationToken cancellationToken = new CancellationToken(),
            IDictionary<string, string> parentState = null) where T : class, ICommand
        {
            if (parentState == null)
                parentState = new Dictionary<string, string>();
            parentState.Merge(cmd.CreateState());

            var messageType = cmd.GetType();
            var operationName = $"PUBLISH COMMAND {messageType.GetTypeShortName()}";

            //if (_fliegel365Options.Value.AllowDevMode)
            //{
            //    if (_devOptions.Value.RaiseCommandFailed)
            //    {
            //        _logger.LogInformation("CommadFailed Notification from DevOptions");

            //        var msg = CommandFailedNotification.Create(cmd,
            //            new Exception("CommandFailed from DevOptions"),
            //            parentState);

            //        await _bus.PublishAsync(msg.CreateMessageWrapper(parentState));

            //        return new MessageResponse
            //        {
            //            UserId = cmd.UserId,
            //            MessageId = cmd.Id
            //        };
            //    }

            //    if (_devOptions.Value.BadRequestForPost)
            //    {
            //        _logger.LogInformation("BadRequest from DevOptions");
            //        throw new ApplicationException("BadRequest from DevOptions");
            //    }
            //}

            try
            {
                await _telemetryAdapter.RunDependencyAsync(operationName,
                    "BUS",
                    async state =>
                    {
                        await _bus.PublishAsync(cmd.CreateMessageWrapper(state));
                    },
                    parentState,
                    _logger);

                var x = new MessageResponse
                {
                    UserId = cmd.UserId,
                    MessageId = cmd.Id
                };
                x.Properties.Merge(parentState);

                return x;
            }
            catch (Exception e) when (LogError(e))
            {
                var x = new MessageResponse
                {
                    Error = e.GetInnerExceptionMessage(),
                    UserId = cmd.UserId,
                    MessageId = cmd.Id
                };
                x.Properties.Merge(parentState);
                return x;
            }
        }

        public async Task<MessageResponse> PublishCommandsAsync<T>(IEnumerable<T> cmds, CancellationToken cancellationToken = new CancellationToken(),
            IDictionary<string, string> state = null) where T : class, ICommand
        {
            var results = new List<MessageResponse>();

            foreach (var cmd in cmds)
            {
                var rsps = await PublishCommandAsync(cmd, cancellationToken, state);
                results.Add(rsps);
            }

            if (results.Any(x => x.IsError))
            {
                return results.FirstOrDefault(x => x.IsError);
            }

            return results.FirstOrDefault();
        }
    }
}
