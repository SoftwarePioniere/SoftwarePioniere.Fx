using System;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Foundatio.Caching;
using Foundatio.Queues;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly bool _useQueue;

        private CancellationToken _cancellationToken;
        private InMemoryEntityStore _initEntityStore;
        
        public EventStoreProjectionContext(ILoggerFactory loggerFactory
            , EventStoreConnectionProvider connectionProvider
            , IEntityStore entityStore
            , IReadModelProjector projector
            , bool useQueue
            , string projectorId
        )
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(GetType());
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            _entityStore = entityStore ?? throw new ArgumentNullException(nameof(entityStore));
            _projector = projector ?? throw new ArgumentNullException(nameof(projector));
            _useQueue = useQueue;

            StreamName = projector.StreamName;

            if (string.IsNullOrEmpty(StreamName))
            {
                throw new InvalidOperationException("no stream name in projector " + projector.GetType().FullName);
            }

            ProjectorId = projectorId;

            if (_useQueue)
            {
                Queue = new InMemoryQueue<ProjectionEventData>(new InMemoryQueueOptions<ProjectionEventData>
                {
                    LoggerFactory = loggerFactory
                });
            }
        }

        private bool InitializationMode { get; set; }

        // private EventStoreStreamCatchUpSubscription _sub;

        public Action LiveProcessingStartedAction { get; set; }

        private IQueue<ProjectionEventData> Queue { get; }
        public long CurrentCheckPoint { get; private set; }


        public IEntityStore EntityStore
        {
            get
            {
                if (InitializationMode && _initEntityStore != null)
                {
                    return _initEntityStore;
                }

                return _entityStore;
            }
        }

        public bool IsLiveProcessing { get; private set; }
        public bool IsReady { get; private set; }
        public string ProjectorId { get; }
        public ProjectionStatus Status { get; set; }
        public string StreamName { get; }

        public async Task StartInitializationModeAsync()
        {
            _logger.LogDebug("StartInitializationMode");

            _initEntityStore = new InMemoryEntityStore(new OptionsWrapper<InMemoryEntityStoreOptions>(
                    new InMemoryEntityStoreOptions
                    {
                        CachingDisabled = true
                    }),
                new InMemoryEntityStoreConnectionProvider(),
                _loggerFactory,
                NullCacheClient.Instance);

            InitializationMode = true;
            IsLiveProcessing = false;
            IsReady = false;

            Status = new ProjectionStatus();
            Status.SetEntityId(ProjectorId);
            Status.LastCheckPoint = -1;
            Status.ProjectorId = ProjectorId;
            Status.StreamName = StreamName;

            await _initEntityStore.InsertItemAsync(Status, _cancellationToken);
        }

        public Task StartSubscription(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("StartSubscription for Projector {ProjectorId} on {Stream}",
                ProjectorId,
                StreamName);
            _cancellationToken = cancellationToken;
            return StartSubscriptionInternal();
        }

        public Task StopInitializationModeAsync()
        {
            _logger.LogDebug("StopInitializationModeAsync");

            InitializationMode = true;
            IsReady = true;
            _initEntityStore = null;

            return Task.CompletedTask;
        }

        internal async Task HandleEventAsync(ProjectionEventData entry)
        {
            _logger.LogTrace("Handled Item {EventNumber} {StreamName} {ProjectorId}",
                entry.EventNumber,
                StreamName,
                ProjectorId);
            CurrentCheckPoint = entry.EventNumber;

            try
            {
                await _projector.ProcessEventAsync(entry.EventData);
                Status.LastCheckPoint = entry.EventNumber;
                Status.ModifiedOnUtc = DateTime.UtcNow;
                Status.ProjectorId = ProjectorId;
                Status.StreamName = StreamName;
                await EntityStore.UpdateItemAsync(Status, _cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Error while Processing Event {EventNumber} from {StreamName} {ProjectorId}",
                    entry.EventNumber,
                    StreamName,
                    ProjectorId);
                throw;
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

            if (_useQueue)
            {
                _logger.LogTrace("Enqueue Event {@0}", desc);
                await Queue.EnqueueAsync(desc);
            }
            else
            {
                await HandleEventAsync(desc);
            }
        }

        private async Task HandleAsync(IQueueEntry<ProjectionEventData> entry)
        {
            _logger.LogDebug("Handled Item {EventNumber}", entry.Value.EventNumber);

            try
            {
                await HandleEventAsync(entry.Value);
                entry.MarkCompleted();
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Error while Processing Event {EventNumber} from {Stream} {ProjectorId}",
                    entry.Value.EventNumber,
                    StreamName,
                    ProjectorId);
                throw;
            }
        }

        private void LiveProcessingStarted(EventStoreCatchUpSubscription sub)
        {
            _logger.LogDebug("LiveProcessingStarted on StreamId {StreamId}, Projector {ProjectorId}",
                sub.StreamId,
                ProjectorId);
            IsLiveProcessing = true;

            LiveProcessingStartedAction?.Invoke();
        }

        private async Task StartSubscriptionInternal()
        {
            _logger.LogDebug("StartSubscriptionInternal for Projector {ProjectorId} on {Stream}",
                ProjectorId,
                StreamName);

            var cred = _connectionProvider.OpsCredentials;
            var src = await _connectionProvider.GetActiveConnection();
            long? lastCheckpoint = null;

            if (Status.LastCheckPoint.HasValue && Status.LastCheckPoint != -1)
            {
                lastCheckpoint = Status.LastCheckPoint;
            }

            if (_useQueue)
            {
                _logger.LogDebug("Start Working in Queue");

                await Queue.StartWorkingAsync(HandleAsync, cancellationToken: _cancellationToken);
            }

            var sub = src.SubscribeToStreamFrom(StreamName,
                lastCheckpoint,
                CatchUpSubscriptionSettings.Default,
                EventAppeared,
                LiveProcessingStarted,
                SubscriptionDropped,
                cred);

            _cancellationToken.Register(sub.Stop);
        }


        private async void SubscriptionDropped(EventStoreCatchUpSubscription sub, SubscriptionDropReason reason,
            Exception ex)
        {
            _logger.LogError(ex,
                "SubscriptionDropped on StreamId {StreamId}, Projector {ProjectorId}, Reason: {Reason}",
                sub.StreamId,
                ProjectorId,
                reason.ToString());
            sub.Stop();

            if (!_cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Re Subscribe Subscription");
                await StartSubscriptionInternal();
            }
        }
    }
}