//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Foundatio.Messaging;
//using Microsoft.ApplicationInsights;
//using Microsoft.Extensions.Logging;
//using SoftwarePioniere.Domain;
//using SoftwarePioniere.Messaging;
//using SoftwarePioniere.Messaging.Notifications;
//using SoftwarePioniere.ReadModel;
//using SoftwarePioniere.Telemetry;

//namespace SoftwarePioniere.Hosting.ApplicationInsights
//{
//    public class AppInsightsTelemetryMessageBusAdapter : IMessageBusAdapter
//    {
//        private readonly IMessageBus _bus;
//        //  private readonly IApplicationLifetime _applicationLifetime;
//        private readonly ILogger _logger;
//        private readonly AppInsightsTelemetryAdapter _telemetryAdapter;


//        public AppInsightsTelemetryMessageBusAdapter(ILoggerFactory loggerFactory, IMessageBus bus
//            , AppInsightsTelemetryAdapter telemetryAdapter
//        )
//        {
//            if (loggerFactory == null)
//            {
//                throw new ArgumentNullException(nameof(loggerFactory));
//            }

//            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
//            _telemetryAdapter = telemetryAdapter ?? throw new ArgumentNullException(nameof(telemetryAdapter));
//            _logger = loggerFactory.CreateLogger(GetType());
//        }


//        public async Task PublishAsync(Type messageType, object message, TimeSpan? delay = null,
//             CancellationToken cancellationToken = new CancellationToken()
//             //,IDictionary<string, string> parentState = null
//             )
//        {
//            var state = new Dictionary<string, string>();
//            //state.Merge(parentState);

//            var operationName = $"PUBLISH {messageType.GetTypeShortName()}";

//            var operationTelemetry = _telemetryAdapter.CreateRequestOperation(operationName, state);

//            try
//            {
//                //state.SetParentRequestId(operationTelemetry.Telemetry.Id);
//                //state.SetOperationId(operationTelemetry.Telemetry.Context.Operation.Id);

//                //using (_logger.BeginScope(state.CreateLoggerScope()))
//                //{
//                _logger.LogInformation(operationName);

//                var sw = Stopwatch.StartNew();
//                _logger.LogDebug($"{operationName} Starting");

//                await _bus.PublishAsync(messageType, message, delay, cancellationToken);

//                //if (!typeof(IMessageWrapper).IsAssignableFrom(messageType) &&
//                //    typeof(IMessage).IsAssignableFrom(messageType))
//                //{
//                //    var imessage = (IMessage)message;
//                //    var created = imessage.CreateMessageWrapper(state);
//                //    await _bus.PublishAsync(created.GetType(), created, delay, cancellationToken);
//                //}
//                //else
//                //{
//                //    await _bus.PublishAsync(messageType, message, delay, cancellationToken);
//                //}

//                sw.Stop();
//                _logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);
//                //}
//            }
//            catch (AuthorizationException e) when (LogError(e, message))
//            {
//                operationTelemetry.Telemetry.Success = false;
//                _telemetryAdapter.TelemetryClient.TrackException(e);
//                throw;
//            }
//            catch (Exception e) when (LogError(e, message))
//            {
//                operationTelemetry.Telemetry.Success = false;
//                _telemetryAdapter.TelemetryClient.TrackException(e);
//            }
//            finally
//            {
//                _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
//            }
//        }

//        public async Task PublishAsync<T>(T message, TimeSpan? delay = null,
//            CancellationToken cancellationToken = new CancellationToken()
//            //,IDictionary<string, string> parentState = null
//            ) where T : class, IMessage
//        {
//            var state = new Dictionary<string, string>();
//            //state.Merge(parentState);

//            var t = typeof(T);

//            var operationName = $"PUBLISH {t.GetTypeShortName()}";

//            var operationTelemetry = _telemetryAdapter.CreateRequestOperation(operationName, state);

//            try
//            {
//                //state.SetParentRequestId(operationTelemetry.Telemetry.Id);

//                //using (_logger.BeginScope(state.CreateLoggerScope()))
//                //{
//                _logger.LogInformation(operationName);

//                var sw = Stopwatch.StartNew();
//                _logger.LogDebug($"{operationName} Starting");

//                //var created = message.CreateMessageWrapper(state);
//                //await _bus.PublishAsync(created, delay);

//                await _bus.PublishAsync(message, delay);

//                sw.Stop();
//                _logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);
//                //}
//            }
//            catch (AuthorizationException e) when (LogError(e, message))
//            {
//                operationTelemetry.Telemetry.Success = false;
//                _telemetryAdapter.TelemetryClient.TrackException(e);
//                throw;
//            }
//            catch (Exception e) when (LogError(e, message))
//            {
//                operationTelemetry.Telemetry.Success = false;
//                _telemetryAdapter.TelemetryClient.TrackException(e);
//            }
//            finally
//            {
//                _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
//            }
//        }

//        public async Task<MessageResponse> PublishCommandAsync<T>(T cmd,
//            CancellationToken cancellationToken = new CancellationToken()
//            //,IDictionary<string, string> parentState = null
//            ) where T : class, ICommand
//        {
//            var state = cmd.CreateState();
//            //state.Merge(parentState);

//            var t = cmd.GetType();
//            var operationName = $"PUBLISH COMMAND {t.GetTypeShortName()}";

//            var operationTelemetry = _telemetryAdapter.CreateRequestOperation(operationName, state);

//            try
//            {
//                //state.SetParentRequestId(operationTelemetry.Telemetry.Id);
//                //state.SetOperationId(operationTelemetry.Telemetry.Context.Operation.Id);

//                //using (_logger.BeginScope(state.CreateLoggerScope()))
//                //{
//                _logger.LogInformation(operationName);


//                //if (_devOptionsSnapshot.Value.RaiseCommandFailed)
//                //{
//                //    _logger.LogInformation("CommadFailed Notification from DevOptions");

//                //    var msg = CommandFailedNotification.Create(cmd,
//                //        new Exception("CommandFailed from DevOptions"),
//                //        state);

//                //    await _bus.PublishAsync(msg);
//                //    //  await _bus.PublishAsync(msg.CreateMessageWrapper(state));

//                //    return new MessageResponse
//                //    {
//                //        UserId = cmd.UserId,
//                //        MessageId = cmd.Id
//                //    };
//                //}

//                //if (_devOptions.Value.PostWithBadRequest)
//                //{
//                //    _logger.LogInformation("BadRequest from DevOptions");
//                //    throw new ApplicationException("BadRequest from DevOptions");
//                //}


//                var sw = Stopwatch.StartNew();
//                _logger.LogDebug($"{operationName} Starting");

//                //await _bus.PublishAsync(cmd.CreateMessageWrapper(state));
//                await _bus.PublishAsync(cmd);

//                sw.Stop();
//                _logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);

//                var x = new MessageResponse
//                {
//                    UserId = cmd.UserId,
//                    MessageId = cmd.Id
//                };
//                //x.Properties.Merge(state);

//                return x;
//                //}
//            }
//            catch (AuthorizationException e) when (LogError(e, cmd))
//            {
//                operationTelemetry.Telemetry.Success = false;
//                _telemetryAdapter.TelemetryClient.TrackException(e);
//                throw;
//            }
//            catch (Exception e) when (LogError(e, cmd))
//            {
//                operationTelemetry.Telemetry.Success = false;
//                _telemetryAdapter.TelemetryClient.TrackException(e);
//                var x = new MessageResponse
//                {
//                    Error = e.GetInnerExceptionMessage(),
//                    UserId = cmd.UserId,
//                    MessageId = cmd.Id
//                };
//                //x.Properties.Merge(state);
//                return x;
//            }
//            finally
//            {
//                _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
//            }
//        }

//        public async Task<MessageResponse> PublishCommandsAsync<T>(IEnumerable<T> cmds,
//            CancellationToken cancellationToken = new CancellationToken()
//            //,IDictionary<string, string> parentState = null
//            ) where T : class, ICommand
//        {
//            //var state = new Dictionary<string, string>();
//            //state.Merge(parentState);

//            var results = new List<MessageResponse>();

//            foreach (var cmd in cmds)
//            {
//                var rsps = await PublishCommandAsync(cmd, cancellationToken);
//                results.Add(rsps);
//            }

//            if (results.Any(x => x.IsError))
//            {
//                return results.FirstOrDefault(x => x.IsError);
//            }

//            return results.FirstOrDefault();
//        }

//        public async Task SubscribeAggregateDomainEvent<TAggregate, TDomainEvent>(
//            Func<TDomainEvent, AggregateTypeInfo<TAggregate>, Task> handler,
//            CancellationToken cancellationToken = new CancellationToken()) where TAggregate : IAggregateRoot
//            where TDomainEvent : class, IDomainEvent
//        {
//            _logger.LogDebug("Subscribing to AggregateEvent {AggregateName} {MessageType}",
//                typeof(TAggregate).GetAggregateName(),
//                typeof(TDomainEvent).GetTypeShortName());

//            //var bus = _bus; // await GetSubscriber();

//            await _bus.SubscribeAsync<AggregateDomainEventMessage>(async (message, token) =>
//             {
//                 if (message.IsAggregate<TAggregate>() && message.IsEventType<TDomainEvent>())
//                 {
//                     var state = new Dictionary<string, string>();
//                     //state.Merge(wrappedMessage.Properties);

//                     var eventType = message.DomainEventType;
//                     var aggregateType = message.AggregateType;
//                     var eventId = message.Id.ToString();
//                     var aggregateName = typeof(TAggregate).GetAggregateName();
//                     var aggregateId = message.AggregateId;
//                     var eventName = typeof(TDomainEvent).Name;

//                     state.AddProperty("EventType", eventType)
//                         .AddProperty("EventId", eventId)
//                         .AddProperty("EventName", eventName)
//                         .AddProperty("AggregateType", aggregateType)
//                         .AddProperty("AggregateName", aggregateName)
//                         .AddProperty("AggregateId", aggregateId)
//                         ;

//                     var operationName = $"HANDLE AGGREGATE EVENT {aggregateName}/{eventName}";

//                     var operationTelemetry = _telemetryAdapter.CreateDependencyOperation(operationName, state);
//                     operationTelemetry.Telemetry.Type = "BUS";


//                     try
//                     {
//                         //state.SetParentRequestId(operationTelemetry.Telemetry.Id);
//                         //state.SetOperationId(operationTelemetry.Telemetry.Context.Operation.Id);

//                         //using (_logger.BeginScope(state.CreateLoggerScope()))
//                         //{
//                         _logger.LogInformation(operationName);

//                         var sw = Stopwatch.StartNew();
//                         _logger.LogDebug($"{operationName} Starting");

//                         await handler(message.GetEvent<TDomainEvent>(), new AggregateTypeInfo<TAggregate>(message.AggregateId));

//                         sw.Stop();
//                         _logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);
//                         //}
//                     }
//                     catch (Exception e) when (LogError(e, message))
//                     {
//                         operationTelemetry.Telemetry.Success = false;
//                         _telemetryAdapter.TelemetryClient.TrackException(e);
//                     }
//                     finally
//                     {
//                         _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
//                     }

//                 }

//             }, cancellationToken);

//            //await bus.SubscribeAsync<MessageWrapper>(
//            //    async (wrappedMessage, token) =>
//            //    {
//            //        if (!wrappedMessage.IsWrappedType<AggregateDomainEventMessage>())
//            //            return;

//            //        var message = wrappedMessage.GetWrappedMessage<AggregateDomainEventMessage>();

//            //        if (!message.IsAggregate<TAggregate>() || !message.IsEventType<TDomainEvent>())
//            //            return;

//            //        var domainEvent = message.GetEvent<TDomainEvent>();

//            //        var eventType = message.DomainEventType;
//            //        var aggregateType = message.AggregateType;
//            //        var eventId = domainEvent.Id.ToString();
//            //        var aggregateName = typeof(TAggregate).GetAggregateName();
//            //        var aggregateId = message.AggregateId;
//            //        var eventName = typeof(TDomainEvent).Name;

//            //        var state = new Dictionary<string, string>();
//            //        //state.Merge(wrappedMessage.Properties);

//            //        state.AddProperty("EventType", eventType)
//            //            .AddProperty("EventId", eventId)
//            //            .AddProperty("EventName", eventName)
//            //            .AddProperty("AggregateType", aggregateType)
//            //            .AddProperty("AggregateName", aggregateName)
//            //            .AddProperty("AggregateId", aggregateId)
//            //            ;

//            //        var operationName = $"HANDLE AGGREGATE EVENT {aggregateName}/{eventName}";

//            //        var operationTelemetry = _telemetryAdapter.CreateDependencyOperation(operationName, state);
//            //        operationTelemetry.Telemetry.Type = "BUS";

//            //        try
//            //        {
//            //            //state.SetParentRequestId(operationTelemetry.Telemetry.Id);
//            //            //state.SetOperationId(operationTelemetry.Telemetry.Context.Operation.Id);

//            //            //using (_logger.BeginScope(state.CreateLoggerScope()))
//            //            //{
//            //            _logger.LogInformation(operationName);

//            //            var sw = Stopwatch.StartNew();
//            //            _logger.LogDebug($"{operationName} Starting");

//            //            await handler(domainEvent, new AggregateTypeInfo<TAggregate>(message.AggregateId), state);

//            //            sw.Stop();
//            //            _logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);
//            //            //}
//            //        }
//            //        catch (Exception e) when (LogError(e, message))
//            //        {
//            //            operationTelemetry.Telemetry.Success = false;
//            //            _telemetryAdapter.TelemetryClient.TrackException(e);
//            //        }
//            //        finally
//            //        {
//            //            _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
//            //        }
//            //    },
//            //    wrapper =>
//            //    {
//            //        if (!wrapper.IsWrappedType<AggregateDomainEventMessage>())
//            //            return false;

//            //        var message = wrapper.GetWrappedMessage<AggregateDomainEventMessage>();

//            //        if (!message.IsAggregate<TAggregate>() || !message.IsEventType<TDomainEvent>())
//            //            return false;

//            //        return true;
//            //    }, cancellationToken);
//        }

//        public async Task SubscribeCommand<T>(Func<T, Task> handler,
//            CancellationToken cancellationToken = new CancellationToken()) where T : class, ICommand
//        {
//            _logger.LogDebug("Subscribing to Command {CommandType}", typeof(T).GetTypeShortName());

//            // var bus = _bus; // await GetSubscriber();
//            await _bus.SubscribeAsync<T>(async (message, token) =>
//                {
//                    //if (!wrappedMessage.IsWrappedType<T>())
//                    //    return;

//                    //var message = wrappedMessage.GetWrappedMessage<T>();

//                    var messageType = typeof(T).GetTypeShortName();

//                    var state = new Dictionary<string, string>();
//                    //state.Merge(wrappedMessage.Properties);

//                    var operationName = $"HANDLE COMMAND {messageType}";

//                    var operationTelemetry = _telemetryAdapter.CreateDependencyOperation(operationName, state);
//                    operationTelemetry.Telemetry.Type = "BUS";

//                    try
//                    {
//                        //state.SetParentRequestId(operationTelemetry.Telemetry.Id);
//                        //state.SetOperationId(operationTelemetry.Telemetry.Context.Operation.Id);


//                        //using (_logger.BeginScope(state.CreateLoggerScope()))
//                        //{
//                        _logger.LogInformation(operationName);

//                        var sw = Stopwatch.StartNew();
//                        _logger.LogDebug($"{operationName} Starting");
//                        await handler(message);

//                        //only send to bus if its coming from external request
//                        if (!string.IsNullOrEmpty(state.GetTraceIdentifier()))
//                        {
//                            await PublishAsync(CommandSucceededNotification.Create(message, state),
//                                cancellationToken: token);
//                        }

//                        sw.Stop();
//                        _logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);
//                        //}
//                    }
//                    catch (Exception e) when (LogError(e, message))
//                    {
//                        operationTelemetry.Telemetry.Success = false;
//                        _telemetryAdapter.TelemetryClient.TrackException(e);

//                        //only send to bus if its coming from external request
//                        if (!string.IsNullOrEmpty(state.GetTraceIdentifier()))
//                        {
//                            await PublishAsync(CommandFailedNotification.Create(message, e, state),
//                                cancellationToken: token);
//                        }
//                    }
//                    finally
//                    {
//                        _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
//                    }
//                },
//                //wrapper =>
//                //{
//                //    if (!wrapper.IsWrappedType<T>())
//                //        return false;
//                //    return true;
//                //},
//                cancellationToken);
//        }

//        public async Task SubscribeMessage<T>(Func<T, Task> handler,
//            CancellationToken cancellationToken = new CancellationToken()) where T : class, IMessage
//        {
//            _logger.LogDebug("Subscribing to Message {MessageType}", typeof(T).GetTypeShortName());


//            await _bus.SubscribeAsync<T>(async (message, token) =>
//                {
//                    //if (!wrappedMessage.IsWrappedType<T>())
//                    //    return;

//                    //var message = wrappedMessage.GetWrappedMessage<T>();

//                    var messageType = typeof(T).GetTypeShortName();

//                    var state = new Dictionary<string, string>();
//                    //state.Merge(wrappedMessage.Properties);

//                    var operationName = $"HANDLE MESSAGE {messageType}";

//                    var operationTelemetry = _telemetryAdapter.CreateDependencyOperation(operationName, state);
//                    operationTelemetry.Telemetry.Type = "BUS";

//                    try
//                    {
//                        //state.SetParentRequestId(operationTelemetry.Telemetry.Id);
//                        //state.SetOperationId(operationTelemetry.Telemetry.Context.Operation.Id);


//                        //using (_logger.BeginScope(state.CreateLoggerScope()))
//                        //{
//                        _logger.LogInformation(operationName);

//                        var sw = Stopwatch.StartNew();
//                        _logger.LogDebug($"{operationName} Starting");

//                        await handler(message);

//                        sw.Stop();
//                        _logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);
//                        //}
//                    }
//                    catch (Exception e) when (LogError(e, message))
//                    {
//                        operationTelemetry.Telemetry.Success = false;
//                        _telemetryAdapter.TelemetryClient.TrackException(e);
//                    }
//                    finally
//                    {
//                        _telemetryAdapter.TelemetryClient.StopOperation(operationTelemetry);
//                    }
//                },
//                //wrapper =>
//                //{
//                //    if (!wrapper.IsWrappedType<T>())
//                //        return false;
//                //    return true;
//                //},
//                cancellationToken);
//        }

//        private bool LogError(Exception ex, object msg)
//        {
//            _logger.LogError(ex, "Ein Fehler ist aufgetreten {Message} {@MessageData}", ex.GetBaseException().Message, msg);
//            return true;
//        }
//    }
//}