﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Foundatio.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Projections;
using SoftwarePioniere.ReadModel;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SoftwarePioniere.EventStore.Projections
{
    public class EventStoreProjectionContext : IProjectionContext
    {
        private readonly EventStoreConnectionProvider _connectionProvider;
        private readonly IEntityStore _entityStore;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        private readonly IReadModelProjector _projector;
        // private readonly bool _useQueue;

        private CancellationToken _cancellationToken;
        private InMemoryEntityStore _initEntityStore;


        private bool _subscriptionDropped;

        public EventStoreProjectionContext(ILoggerFactory loggerFactory
            , EventStoreConnectionProvider connectionProvider
            , IEntityStore entityStore
            , IReadModelProjector projector
            // , bool useQueue
            , string projectorId
        )
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(GetType());
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            _entityStore = entityStore ?? throw new ArgumentNullException(nameof(entityStore));
            _projector = projector ?? throw new ArgumentNullException(nameof(projector));
            // _useQueue = useQueue;

            StreamName = projector.StreamName;

            if (string.IsNullOrEmpty(StreamName))
            {
                throw new InvalidOperationException("no stream name in projector " + projector.GetType().FullName);
            }

            ProjectorId = projectorId;

            // if (_useQueue)
            // {
            //     Queue = new InMemoryQueue<ProjectionEventData>(new InMemoryQueueOptions<ProjectionEventData>
            //     {
            //         LoggerFactory = loggerFactory
            //     });
            // }
        }

        // private EventStoreStreamCatchUpSubscription _sub;

        public Action LiveProcessingStartedAction { get; set; }

        // private IQueue<ProjectionEventData> Queue { get; }
        public long CurrentCheckPoint { get; private set; }


        public IEntityStore EntityStore
        {
            get
            {
                if (IsInitializing && _initEntityStore != null)
                {
                    return _initEntityStore;
                }

                return _entityStore;
            }
        }

        public bool IsInitializing { get; private set; }

        public bool IsLiveProcessing { get; private set; }
        public bool IsReady { get; private set; }
        public string ProjectorId { get; }
        public ProjectionStatus Status { get; set; }
        public string StreamName { get; }

        public async Task StartInitializationModeAsync()
        {
            _logger.LogDebug("StartInitializationMode");

            _initEntityStore = new InMemoryEntityStore(new OptionsWrapper<InMemoryEntityStoreOptions>(
                    new InMemoryEntityStoreOptions()),
                new InMemoryEntityStoreConnectionProvider(),
                _loggerFactory,
                NullCacheClient.Instance,
                new OptionsWrapper<CacheOptions>(
                    new CacheOptions
                    {
                        CachingDisabled = true
                    }
                )
            );

            IsInitializing = true;
            IsLiveProcessing = false;
            IsReady = false;

            Status = new ProjectionStatus();
            Status.SetEntityId(ProjectorId);
            Status.LastCheckPoint = -1;
            Status.ProjectorId = ProjectorId;
            Status.StreamName = StreamName;

            await EntityStore.InsertItemAsync(Status, _cancellationToken).ConfigureAwait(false);
        }

        public Task StartSubscription(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("StartSubscription for Projector {ProjectorId} on {Stream}",
                ProjectorId,
                StreamName);
            _cancellationToken = cancellationToken;
            StartSubscriptionInternal();
            return Task.CompletedTask;
        }

        public Task StopInitializationModeAsync()
        {
            _logger.LogDebug("StopInitializationModeAsync");

            IsInitializing = false;
            IsReady = true;
            _initEntityStore = null;

            return Task.CompletedTask;
        }

        public Task UpdateStreamStatusAsync()
        {
            return EntityStore.UpdateItemAsync(Status, _cancellationToken);
        }

        protected bool LogError(Exception ex)
        {
            _logger.LogError(ex, ex.GetBaseException().Message);
            return true;
        }

        internal async Task HandleEventAsync(ProjectionEventData entry)
        {
            var state = new Dictionary<string, object>
            {
                {"EventType", entry.EventData.GetType().FullName},
                {"EventId", entry.EventData.Id},
                {"EventNumber", entry.EventNumber},
                {"ProjectorType", _projector.GetType().FullName},
                {"StreamName", StreamName}
            };

            using (_logger.BeginScope(state))
            {
                var sw = Stopwatch.StartNew();

                _logger.LogDebug("HandleEventAsync Item {EventNumber} {StreamName} Started",
                    entry.EventNumber,
                    StreamName);

                if (entry.EventNumber > Status.LastCheckPoint)
                {
                    CurrentCheckPoint = entry.EventNumber;

                    try
                    {
                        await _projector.ProcessEventAsync(entry.EventData).ConfigureAwait(false);
                        Status.LastCheckPoint = entry.EventNumber;
                        Status.ModifiedOnUtc = DateTime.UtcNow;

                        if (!IsInitializing)
                        {
                            await EntityStore.UpdateItemAsync(Status, _cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (Exception e) when (LogError(e))
                    {
                        _logger.LogError(e,
                            "Error while Processing Event {EventNumber} from {StreamName} {ProjectorId}",
                            entry.EventNumber,
                            StreamName,
                            ProjectorId);
                    }
                }
                else
                {
                    _logger.LogWarning("Duplicate Event Handling  {EventNumber} {StreamName}",
                        entry.EventNumber,
                        StreamName);
                }

                sw.Stop();
                _logger.LogDebug("HandleEventAsync Item {EventNumber} {StreamName} finished in {Elapsed} ms",
                    entry.EventNumber,
                    StreamName,
                    sw.ElapsedMilliseconds);
            }
        }

        private async Task EventAppeared(EventStoreCatchUpSubscription sub, ResolvedEvent evt)
        {
            _logger.LogTrace("EventAppeared {SubscriptionName} {Stream} Projector {ProjectorId}",
                sub.SubscriptionName,
                sub.StreamId,
                ProjectorId);

            var de = evt.Event.ToDomainEvent();
            var desc = new ProjectionEventData
            {
                EventData = de,
                EventNumber = evt.OriginalEventNumber
            };

            // if (_useQueue)
            // {
            //     _logger.LogTrace("Enqueue Event {@0}", desc);
            //     await Queue.EnqueueAsync(desc).ConfigureAwait(false);
            // }
            // else
            // {
            await HandleEventAsync(desc).ConfigureAwait(false);
            // }
        }

        // private async Task HandleAsync(IQueueEntry<ProjectionEventData> entry)
        // {
        //     _logger.LogDebug("Handled Item {EventNumber}", entry.Value.EventNumber);
        //
        //     try
        //     {
        //         await HandleEventAsync(entry.Value).ConfigureAwait(false);
        //         entry.MarkCompleted();
        //     }
        //     catch (Exception e) when (LogError(e))
        //     {
        //         _logger.LogError(e,
        //             "Error while Processing Event {EventNumber} from {Stream} {ProjectorId}",
        //             entry.Value.EventNumber,
        //             StreamName,
        //             ProjectorId);
        //     }
        // }

        private void LiveProcessingStarted(EventStoreCatchUpSubscription sub)
        {
            _logger.LogDebug("LiveProcessingStarted on StreamId {StreamId}, Projector {ProjectorId}",
                sub.StreamId,
                ProjectorId);
            IsLiveProcessing = true;

            LiveProcessingStartedAction?.Invoke();
        }

        private void StartSubscriptionInternal()
        {
            _logger.LogDebug("StartSubscriptionInternal for Projector {ProjectorId} on {Stream}",
                ProjectorId,
                StreamName);

            var cred = _connectionProvider.OpsCredentials;
            //var src = await _connectionProvider.GetActiveConnection().ConfigureAwait(false);
            var src = _connectionProvider.GetActiveConnection();

            src.Connected += (s, e) =>
            {
                if (_subscriptionDropped)
                {
                    _logger.LogDebug("Reconnected after Subscription Dropped. Resubscribe to Subscription");
                    _subscriptionDropped = false;
                    SubscribeInternal(src, cred);
                }
            };

            SubscribeInternal(src, cred);
        }

        private void SubscribeInternal(IEventStoreConnection src, UserCredentials cred)
        {
            long? lastCheckpoint = null;

            if (Status.LastCheckPoint.HasValue && Status.LastCheckPoint != -1)
            {
                lastCheckpoint = Status.LastCheckPoint;
            }

            // if (_useQueue)
            // {
            //     _logger.LogDebug("Start Working in Queue");
            //     await Queue.StartWorkingAsync(HandleAsync, cancellationToken: _cancellationToken).ConfigureAwait(false);
            // }

            var sub = src.SubscribeToStreamFrom(StreamName,
                lastCheckpoint,
                CatchUpSubscriptionSettings.Default,
                EventAppeared,
                LiveProcessingStarted,
                SubscriptionDropped,
                cred);

            _cancellationToken.Register(sub.Stop);
        }

        private void SubscriptionDropped(EventStoreCatchUpSubscription sub, SubscriptionDropReason reason,
            Exception ex)
        {
            _logger.LogError(ex,
                "SubscriptionDropped on StreamId {StreamId}, Projector {ProjectorId}, Reason: {Reason}",
                sub.StreamId,
                ProjectorId,
                reason.ToString());

            if (!_cancellationToken.IsCancellationRequested)
            {
                // _logger.LogInformation("Re Subscribe Subscription");
                _subscriptionDropped = true;

                // await Task.Delay(TimeSpan.FromSeconds(2), CancellationToken.None)
                //     .ContinueWith(task => StartSubscriptionInternal(), CancellationToken.None);
                //await StartSubscriptionInternal().ConfigureAwait(false);
            }
        }
    }
}