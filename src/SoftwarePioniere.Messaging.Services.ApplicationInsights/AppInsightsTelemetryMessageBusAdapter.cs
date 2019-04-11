using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.DomainModel;
using SoftwarePioniere.Messaging.Notifications;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Messaging.Services.ApplicationInsights
{
    public class AppInsightsTelemetryMessageBusAdapter : IMessageBusAdapter
    {
        private readonly IMessageBus2 _bus;
        private readonly ILogger _logger;
        private readonly AppInsightsTelemetryAdapter _telemetryAdapter;


        public AppInsightsTelemetryMessageBusAdapter(ILoggerFactory loggerFactory, IMessageBus2 bus
            , AppInsightsTelemetryAdapter telemetryAdapter
        //  , IApplicationLifetime applicationLifetime
        )
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _bus = bus ?? throw new ArgumentNullException(nameof(bus));

            _telemetryAdapter = telemetryAdapter ?? throw new ArgumentNullException(nameof(telemetryAdapter));

            _logger = loggerFactory.CreateLogger(GetType());

            //   _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));

            //_internalBus = new Lazy<IMessageBus>(() => new InMemoryMessageBus(builder => builder.LoggerFactory(loggerFactory).TaskQueueMaxItems(Int32.MaxValue)), LazyThreadSafetyMode.ExecutionAndPublication);
        }
        //private readonly Lazy<IMessageBus> _internalBus;
        //private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        //private async Task<IMessageSubscriber> GetSubscriber()
        //{

        //    if (!_internalBus.IsValueCreated)
        //    {
        //        //https://blog.cdemi.io/async-waiting-inside-c-sharp-locks/
        //        await SemaphoreSlim.WaitAsync();

        //        try
        //        {
        //            //die events aus dem externen bus an den internen weiter geben, damit sie schneller gefiltert werden könnne
        //            await _bus.SubscribeAsync<MessageWrapper>(async (wrappedMessage, token) =>
        //                {
        //                    await _internalBus.Value.PublishAsync(wrappedMessage);
        //                }
        //                , _applicationLifetime.ApplicationStopping);
        //        }
        //        finally
        //        {
        //            //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
        //            //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
        //            SemaphoreSlim.Release();
        //        }
        //    }

        //    return _internalBus.Value;
        //}


        public async Task PublishAsync(Type messageType, object message, TimeSpan? delay = null,
            CancellationToken cancellationToken = new CancellationToken(),
            IDictionary<string, string> parentState = null)
        {
            var state = new Dictionary<string, string>();
            state.Merge(parentState);

            var operationName = $"PUBLISH {messageType.GetTypeShortName()}";

            var operationTelemetry = _telemetryAdapter.CreateRequestOperation(operationName, state);

            try
            {
                state.SetParentRequestId(operationTelemetry.Telemetry.Id);

                using (_logger.BeginScope(state.CreateLoggerScope()))
                {
                    _logger.LogInformation(operationName);

                    var sw = Stopwatch.StartNew();
                    _logger.LogDebug($"{operationName} Starting");

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

                    sw.Stop();
                    _logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);
                }
            }
            catch (AuthorizationException e) when (LogError(e))
            {
                operationTelemetry.Telemetry.Success = false;
                _telemetryAdapter.TelemetryClient.TrackException(e);
                throw;
            }
            catch (Exception e) when (LogError(e))
            {
                operationTelemetry.Telemetry.Success = false;
                _telemetryAdapter.TelemetryClient.TrackException(e);
            }
            finally
            {
                _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
            }
        }

        public async Task PublishAsync<T>(T message, TimeSpan? delay = null,
            CancellationToken cancellationToken = new CancellationToken(),
            IDictionary<string, string> parentState = null) where T : class, IMessage
        {
            var state = new Dictionary<string, string>();
            state.Merge(parentState);

            var messageType = typeof(T);

            var operationName = $"PUBLISH {messageType.GetTypeShortName()}";

            var operationTelemetry = _telemetryAdapter.CreateRequestOperation(operationName, state);

            try
            {
                state.SetParentRequestId(operationTelemetry.Telemetry.Id);

                using (_logger.BeginScope(state.CreateLoggerScope()))
                {
                    _logger.LogInformation(operationName);

                    var sw = Stopwatch.StartNew();
                    _logger.LogDebug($"{operationName} Starting");

                    var created = message.CreateMessageWrapper(state);
                    await _bus.PublishAsync(created, delay);

                    sw.Stop();
                    _logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);
                }
            }
            catch (AuthorizationException e) when (LogError(e))
            {
                operationTelemetry.Telemetry.Success = false;
                _telemetryAdapter.TelemetryClient.TrackException(e);
                throw;
            }
            catch (Exception e) when (LogError(e))
            {
                operationTelemetry.Telemetry.Success = false;
                _telemetryAdapter.TelemetryClient.TrackException(e);
            }
            finally
            {
                _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
            }
        }

        public async Task<MessageResponse> PublishCommandAsync<T>(T cmd,
            CancellationToken cancellationToken = new CancellationToken(),
            IDictionary<string, string> parentState = null) where T : class, ICommand
        {
            var state = cmd.CreateState();
            state.Merge(parentState);

            var messageType = cmd.GetType();
            var operationName = $"PUBLISH COMMAND {messageType.GetTypeShortName()}";

            var operationTelemetry = _telemetryAdapter.CreateRequestOperation(operationName, state);

            try
            {
                state.SetParentRequestId(operationTelemetry.Telemetry.Id);

                using (_logger.BeginScope(state.CreateLoggerScope()))
                {
                    _logger.LogInformation(operationName);

                    //if (_fliegel365Options.Value.AllowDevMode)
                    //{
                    //    if (_devOptions.Value.RaiseCommandFailed)
                    //    {
                    //        _logger.LogInformation("CommadFailed Notification from DevOptions");

                    //        var msg = CommandFailedNotification.Create(cmd,
                    //            new Exception("CommandFailed from DevOptions"),
                    //            state);

                    //        await _bus.PublishAsync(msg.CreateMessageWrapper(state));

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

                    var sw = Stopwatch.StartNew();
                    _logger.LogDebug($"{operationName} Starting");

                    await _bus.PublishAsync(cmd.CreateMessageWrapper(state));

                    sw.Stop();
                    _logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);

                    var x = new MessageResponse
                    {
                        UserId = cmd.UserId,
                        MessageId = cmd.Id
                    };
                    x.Properties.Merge(state);

                    return x;
                }
            }
            catch (AuthorizationException e) when (LogError(e))
            {
                operationTelemetry.Telemetry.Success = false;
                _telemetryAdapter.TelemetryClient.TrackException(e);
                throw;
            }
            catch (Exception e) when (LogError(e))
            {
                operationTelemetry.Telemetry.Success = false;
                _telemetryAdapter.TelemetryClient.TrackException(e);
                var x = new MessageResponse
                {
                    Error = e.GetInnerExceptionMessage(),
                    UserId = cmd.UserId,
                    MessageId = cmd.Id
                };
                x.Properties.Merge(state);
                return x;
            }
            finally
            {
                _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
            }
        }

        public async Task<MessageResponse> PublishCommandsAsync<T>(IEnumerable<T> cmds,
            CancellationToken cancellationToken = new CancellationToken(),
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

        public async Task SubscribeAggregateDomainEvent<TAggregate, TDomainEvent>(
            Func<TDomainEvent, AggregateTypeInfo<TAggregate>, IDictionary<string, string>, Task> handler,
            CancellationToken cancellationToken = new CancellationToken()) where TAggregate : IAggregateRoot
            where TDomainEvent : class, IDomainEvent
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

                    var operationName = $"HANDLE AGGREGATE EVENT {aggregateName}/{eventName}";

                    var operationTelemetry = _telemetryAdapter.CreateDependencyOperation(operationName, state);
                    operationTelemetry.Telemetry.Type = "BUS";

                    try
                    {
                        state.SetParentRequestId(operationTelemetry.Telemetry.Id);

                        using (_logger.BeginScope(state.CreateLoggerScope()))
                        {
                            _logger.LogInformation(operationName);

                            var sw = Stopwatch.StartNew();

                            _logger.LogDebug(
                                "HandleAggregateEvent Started {AggregateName} {MessageType} {MessageId}",
                                aggregateName,
                                eventType,
                                eventId);

                            await handler(domainEvent,
                                new AggregateTypeInfo<TAggregate>(message.AggregateId),
                                state.Copy());

                            sw.Stop();
                            _logger.LogDebug(
                                "HandleAggregateEvent Finished {AggregateName} {MessageType} {MessageId} in {Elapsed:0.0000} ms ",
                                aggregateName,
                                eventType,
                                eventId,
                                sw.ElapsedMilliseconds);
                        }
                    }
                    catch (Exception e) when (LogError(e))
                    {
                        operationTelemetry.Telemetry.Success = false;
                        _telemetryAdapter.TelemetryClient.TrackException(e);
                    }
                    finally
                    {
                        _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
                    }
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

        public async Task SubscribeCommand<T>(Func<T, IDictionary<string, string>, Task> handler,
            CancellationToken cancellationToken = new CancellationToken()) where T : class, ICommand
        {
            _logger.LogDebug("Subscribing to Command {CommandType}", typeof(T).GetTypeShortName());

            var bus = _bus; // await GetSubscriber();
            await bus.SubscribeAsync<MessageWrapper>(async (wrappedMessage, token) =>
                {
                    if (!wrappedMessage.IsWrappedType<T>())
                        return;

                    var message = wrappedMessage.GetWrappedMessage<T>();

                    var messageType = typeof(T).GetTypeShortName();
                    var state = wrappedMessage.Properties ?? new Dictionary<string, string>();

                    var operationName = $"HANDLE COMMAND {messageType}";

                    var operationTelemetry = _telemetryAdapter.CreateDependencyOperation(operationName, state);
                    operationTelemetry.Telemetry.Type = "BUS";

                    try
                    {
                        state.SetParentRequestId(operationTelemetry.Telemetry.Id);

                        using (_logger.BeginScope(state.CreateLoggerScope()))
                        {
                            _logger.LogInformation(operationName);

                            var sw = Stopwatch.StartNew();
                            _logger.LogDebug("HandleCommand Started {CommandType} {MessageId}",
                                messageType,
                                message.Id);
                            await handler(message, state.Copy());

                            //only send to bus if its coming from external request
                            if (!string.IsNullOrEmpty(state.GetTraceIdentifier()))
                            {
                                await PublishAsync(CommandSucceededNotification.Create(message, state),
                                    cancellationToken: token,
                                    parentState: state);
                            }

                            sw.Stop();
                            _logger.LogInformation(
                                "HandleCommand Finished {CommandType} {MessageId} in {Elapsed:0.0000} ms ",
                                messageType,
                                message.Id,
                                sw.ElapsedMilliseconds);
                        }
                    }
                    catch (Exception e) when (LogError(e))
                    {
                        operationTelemetry.Telemetry.Success = false;
                        _telemetryAdapter.TelemetryClient.TrackException(e);

                        //only send to bus if its coming from external request
                        if (!string.IsNullOrEmpty(state.GetTraceIdentifier()))
                        {
                            await PublishAsync(CommandFailedNotification.Create(message, e, state),
                                cancellationToken: token,
                                parentState: state);
                        }
                    }
                    finally
                    {
                        _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
                    }
                },
                wrapper =>
                {
                    if (!wrapper.IsWrappedType<T>())
                        return false;
                    return true;
                },
                cancellationToken);
        }

        public async Task SubscribeMessage<T>(Func<T, IDictionary<string, string>, Task> handler,
            CancellationToken cancellationToken = new CancellationToken()) where T : class, IMessage
        {
            _logger.LogDebug("Subscribing to Message {MessageType}", typeof(T).GetTypeShortName());

            var bus = _bus; // await GetSubscriber();
            await bus.SubscribeAsync<MessageWrapper>(async (wrappedMessage, token) =>
                {
                    if (!wrappedMessage.IsWrappedType<T>())
                        return;

                    var message = wrappedMessage.GetWrappedMessage<T>();

                    var messageType = typeof(T).GetTypeShortName();
                    var state = wrappedMessage.Properties ?? new Dictionary<string, string>();
                    var operationName = $"HANDLE MESSAGE {messageType}";

                    var operationTelemetry = _telemetryAdapter.CreateDependencyOperation(operationName, state);
                    operationTelemetry.Telemetry.Type = "BUS";

                    try
                    {
                        state.SetParentRequestId(operationTelemetry.Telemetry.Id);

                        using (_logger.BeginScope(state.CreateLoggerScope()))
                        {
                            _logger.LogInformation(operationName);

                            var sw = Stopwatch.StartNew();
                            _logger.LogDebug("HandleMessage Started {MessageType} {MessageId}",
                                messageType,
                                message.Id);
                            await handler(message
                                , state.Copy());
                            sw.Stop();
                            _logger.LogInformation(
                                "HandleMessage Finished {MessageType} {MessageId} in {Elapsed:0.0000} ms ",
                                messageType,
                                message.Id,
                                sw.ElapsedMilliseconds);
                        }
                    }
                    catch (Exception e) when (LogError(e))
                    {
                        operationTelemetry.Telemetry.Success = false;
                        _telemetryAdapter.TelemetryClient.TrackException(e);
                    }
                    finally
                    {
                        _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
                    }
                },
                wrapper =>
                {
                    if (!wrapper.IsWrappedType<T>())
                        return false;
                    return true;
                },
                cancellationToken);
        }


        private bool LogError(Exception ex)
        {
            _logger.LogError(ex, "Ein Fehler ist aufgetreten {Message}", ex.GetBaseException().Message);
            return true;
        }
    }
}