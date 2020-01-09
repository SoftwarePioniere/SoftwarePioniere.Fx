using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            try
            {
                return _bus.PublishAsync(messageType, message, delay, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on Publish Message {MessageType} {@Message}", messageType, message);
                return Task.CompletedTask;
            }
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
                var state = new Dictionary<string, object>
                {
                    {"MessageType", message.GetType().FullName},
                    //{"EventId", entry.EventData.Id},
                    //{"EventNumber", entry.EventNumber},
                    //{"ProjectorType", _projector.GetType().FullName},
                    //{"StreamName", StreamName}
                };

                using (_logger.BeginScope(state))
                {
                    var sw = Stopwatch.StartNew();
                    _logger.LogDebug("HandleMessage started");
                    try
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
                    }
                    catch (Exception e) when (LogError(e))
                    {
                        _logger.LogError(e, "Error on handling Message {MessageType} {@Message}", typeof(T), message);
                    }

                    sw.Stop();
                    _logger.LogDebug("HandleMessage finished in {Elapsed} ms", sw.ElapsedMilliseconds);

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
                    using (_logger.BeginScope(state.CreateLoggerScope()))
                    {
                        var sw = Stopwatch.StartNew();
                        _logger.LogDebug("SubscribeCommand started");

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
                        catch (Exception e) when (LogError(e))
                        {
                            _logger.LogError(e, "Error on handling Command {MessageType} {@Message}", typeof(T), message);
                            await PublishAsync(CommandFailedNotification.Create(message, e, state), cancellationToken: token);
                        }

                        sw.Stop();
                        _logger.LogDebug("SubscribeCommand finished in {Elapsed} ms", sw.ElapsedMilliseconds);

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
                try
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

                        var state = new Dictionary<string, object>
                        {
                            {"DomainEventType", message.GetType().FullName},
                            {"AggregateType",  message.AggregateType},
                            {"EventId", domainEvent.Id},
                            {"AggregateName", typeof(TAggregate).GetAggregateName()},
                            {"AggregateId", message.AggregateId},
                            {"EventName", typeof(TDomainEvent).Name},
                        };

                        using (_logger.BeginScope(state))
                        {
                            var sw = Stopwatch.StartNew();
                            _logger.LogDebug("HandleDomainEvent started");

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

                            sw.Stop();
                            _logger.LogDebug("HandleDomainEvent finished in {Elapsed} ms", sw.ElapsedMilliseconds);

                        }
                    }
                }
                catch (Exception e) when (LogError(e))
                {
                    _logger.LogError(e, "Error on handling AggregateEvent {AggregateName} {MessageType} {@Message}",
                        typeof(TAggregate).GetAggregateName(),
                        typeof(TDomainEvent).GetTypeShortName(),
                        message);
                }

            }, cancellationToken);
        }

        public async Task<MessageResponse> PublishCommandAsync<T>(T cmd, CancellationToken cancellationToken = default
        ) where T : class, ICommand
        {

            var state = cmd.CreateState();
            using (_logger.BeginScope(state.CreateLoggerScope()))
            {
                var sw = Stopwatch.StartNew();
                _logger.LogDebug("PublishCommandAsync started");

                var x = new MessageResponse
                {
                    UserId = cmd.UserId,
                    MessageId = cmd.Id
                };
                x.Properties.Merge(cmd.CreateState());

                try
                {
                    await _bus.PublishAsync(cmd);

                }
                catch (Exception e) when (LogError(e))
                {
                    x.Error = e.GetInnerExceptionMessage();
                }

                sw.Stop();
                _logger.LogDebug("PublishCommandAsync finished in {Elapsed} ms", sw.ElapsedMilliseconds);

                return x;
            }
        }

        public async Task<MessageResponse> PublishCommandsAsync<T>(IEnumerable<T> cmds, CancellationToken cancellationToken = default
        ) where T : class, ICommand
        {

            var sw = Stopwatch.StartNew();
            _logger.LogDebug("PublishCommandsAsync started");

            var tasks = cmds.Select(cmd => PublishCommandAsync(cmd, cancellationToken)).ToList();
            var results = await Task.WhenAll(tasks);

            if (results.Any(x => x.IsError)) return results.FirstOrDefault(x => x.IsError);

            sw.Stop();
            _logger.LogDebug("PublishCommandsAsync finished in {Elapsed} ms", sw.ElapsedMilliseconds);

            return results.FirstOrDefault();

        }

        private bool LogError(Exception ex)
        {
            _logger.LogError(ex, ex.GetBaseException().Message);
            return true;
        }
    }
}