using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Lock;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.Messaging.Notifications;
using SoftwarePioniere.Telemetry;

namespace SoftwarePioniere.Messaging
{
    public class DefaultMessageBusAdapter : IMessageBusAdapter
    {
        private readonly ISopiApplicationLifetime _applicationLifetime;
        private readonly IMessageBus _bus;
        private readonly ILockProvider _lockProvider;
        private readonly ILogger _logger;

        public DefaultMessageBusAdapter(ILoggerFactory loggerFactory, IMessageBus bus, ISopiApplicationLifetime applicationLifetime, ILockProvider lockProvider)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(GetType());

            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _applicationLifetime = applicationLifetime;
            _lockProvider = lockProvider;
        }

        public Task PublishAsync(Type messageType, object message, TimeSpan? delay = null, CancellationToken cancellationToken = default)
        {
            return _bus.PublishAsync(messageType, message, delay, cancellationToken);
        }

        public Task PublishAsync<T>(T message, TimeSpan? delay = null
            , CancellationToken cancellationToken = new CancellationToken()
        ) where T : class, IMessage
        {
            var messageType = typeof(T);
            return _bus.PublishAsync(messageType, message, delay, cancellationToken);
        }

        public async Task SubscribeMessage<T>(Func<T, Task> handler, CancellationToken cancellationToken = default
            , Func<T, string> lockId = null
        ) where T : class, IMessage
        {
            _logger.LogDebug("Subscribing to Message {MessageType}", typeof(T).GetTypeShortName());
            var bus = _bus;

            await bus.SubscribeAsync<T>(async (message, token) =>
            {
                if (lockId != null)
                {
                    var lockResource = lockId(message);
                    _logger.LogDebug("Handle Message with Lock {LockId}", lockResource);
                    await _lockProvider.TryUsingAsync(lockResource, token1 => handler(message), cancellationToken: cancellationToken);
                }
                else
                {
                    await handler(message);
                }
            }, cancellationToken);
        }

        public async Task SubscribeCommand<T>(Func<T, Task> handler, CancellationToken cancellationToken = default, Func<T, string> lockId = null) where T : class, ICommand
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_applicationLifetime.Stopped, cancellationToken);

            _logger.LogDebug("Subscribing to Command {CommandType}", typeof(T).GetTypeShortName());
            var bus = _bus;
            await bus.SubscribeAsync<T>(async (message, token) =>
                {
                    var state = message.CreateState();
                    try
                    {
                        if (lockId != null)
                        {
                            var lockResource = lockId(message);
                            _logger.LogDebug("Handle Command with Lock {LockId}", lockResource);
                            await _lockProvider.TryUsingAsync(lockResource, token1 => handler(message), cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await handler(message);
                        }

                        await PublishAsync(CommandSucceededNotification.Create(message, state), cancellationToken: token);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error on handling Command {MessageType} @{Message}", typeof(T), message);
                        await PublishAsync(CommandFailedNotification.Create(message, e, state), cancellationToken: token);
                    }
                },
                cts.Token);
        }

        public async Task SubscribeAggregateDomainEvent<TAggregate, TDomainEvent>(Func<TDomainEvent, AggregateTypeInfo<TAggregate>, Task> handler,
            CancellationToken cancellationToken = default
            , Func<TDomainEvent, AggregateTypeInfo<TAggregate>, string> lockId = null
        ) where TAggregate : IAggregateRoot where TDomainEvent : class, IDomainEvent
        {
            _logger.LogDebug("Subscribing to AggregateEvent {AggregateName} {MessageType}",
                typeof(TAggregate).GetAggregateName(),
                typeof(TDomainEvent).GetTypeShortName());


            await _bus.SubscribeAsync<AggregateDomainEventMessage>(async (message, token) =>
            {
                if (message.IsAggregate<TAggregate>() && message.IsEventType<TDomainEvent>())
                {
                    var domainEvent = message.GetEvent<TDomainEvent>();

                    //var eventType = message.DomainEventType;
                    //var aggregateType = message.AggregateType;
                    //var eventId = domainEvent.Id.ToString();
                    //var aggregateName = typeof(TAggregate).GetAggregateName();
                    //var aggregateId = message.AggregateId;
                    //var eventName = typeof(TDomainEvent).Name;

                    //var state = wrappedMessage.Properties ?? new Dictionary<string, string>();
                    //state.AddProperty("EventType", eventType)
                    //    .AddProperty("EventId", eventId)
                    //    .AddProperty("EventName", eventName)
                    //    .AddProperty("AggregateType", aggregateType)
                    //    .AddProperty("AggregateName", aggregateName)
                    //    .AddProperty("AggregateId", aggregateId)
                    //    ;

                    Task Exc()
                    {
                        return handler(domainEvent, new AggregateTypeInfo<TAggregate>(message.AggregateId));
                    }

                    if (lockId != null)
                    {
                        var lockResource = lockId(domainEvent, new AggregateTypeInfo<TAggregate>(message.AggregateId));
                        _logger.LogDebug("Handle Domain Event with Lock {LockId}", lockResource);
                        await _lockProvider.TryUsingAsync(lockResource, token1 => Exc(), cancellationToken: cancellationToken);
                    }
                    else
                    {
                        _logger.LogDebug("Handle Domain Event without Locking");
                        await Exc();
                    }
                }
            }, cancellationToken);
        }

        public async Task<MessageResponse> PublishCommandAsync<T>(T cmd, CancellationToken cancellationToken = default
        ) where T : class, ICommand
        {
            try
            {
                await _bus.PublishAsync(cmd);

                var x = new MessageResponse
                {
                    UserId = cmd.UserId,
                    MessageId = cmd.Id
                };
                x.Properties.Merge(cmd.CreateState());

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
                x.Properties.Merge(cmd.CreateState());
                return x;
            }
        }

        public async Task<MessageResponse> PublishCommandsAsync<T>(IEnumerable<T> cmds, CancellationToken cancellationToken = default
        ) where T : class, ICommand
        {
            var results = new List<MessageResponse>();

            foreach (var cmd in cmds)
            {
                var rsps = await PublishCommandAsync(cmd, cancellationToken);
                results.Add(rsps);
            }

            if (results.Any(x => x.IsError)) return results.FirstOrDefault(x => x.IsError);

            return results.FirstOrDefault();
        }

        private bool LogError(Exception ex)
        {
            _logger.LogError(ex, "Ein Fehler ist aufgetreten {Message}", ex.GetBaseException().Message);
            return true;
        }
    }
}