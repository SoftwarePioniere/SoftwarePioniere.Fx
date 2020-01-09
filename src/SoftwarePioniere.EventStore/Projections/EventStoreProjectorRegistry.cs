using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Foundatio.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using SoftwarePioniere.EventStore.Domain;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Projections;
using SoftwarePioniere.ReadModel;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SoftwarePioniere.EventStore.Projections
{
    public class EventStoreProjectorRegistry : IProjectorRegistry
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly EventStoreConnectionProvider _connectionProvider;
        private readonly IEnumerable<IReadModelProjector> _projectors;
        private readonly IEntityStore _entityStore;
        private readonly ICacheClient _cache;
        private readonly ILogger _logger;
        private readonly ProjectionOptions _options;

        public EventStoreProjectorRegistry(ILoggerFactory loggerFactory
            , EventStoreConnectionProvider connectionProvider
            , IServiceProvider serviceProvider
            , IEntityStore entityStore
            , ICacheClient cache
            , IOptions<ProjectionOptions> options
        )
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));

            _logger = loggerFactory.CreateLogger(GetType());
            _projectors = serviceProvider.GetServices<IReadModelProjector>();
            _entityStore = entityStore ?? throw new ArgumentNullException(nameof(entityStore));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = options.Value;
        }

  
        private async Task<ProjectionStatus> ReadStreamAsync(string stream, EventStoreProjectionContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogDebug("ReadFromStreamAsync {Stream} {ProjectorId}", stream, context.ProjectorId);
            var sw = Stopwatch.StartNew();

            var cred = _connectionProvider.OpsCredentials;
            var src = await _connectionProvider.GetActiveConnection().ConfigureAwait(false);

            StreamEventsSlice slice;

            long sliceStart = StreamPosition.Start;

            do
            {
                _logger.LogTrace("Reading Slice from {0}", sliceStart);

                slice = await src.ReadStreamEventsForwardAsync(stream, sliceStart, _options.InitReadCount, true, cred).ConfigureAwait(false);
                _logger.LogTrace("Next Event: {0} , IsEndOfStream: {1}", slice.NextEventNumber, slice.IsEndOfStream);

                sliceStart = slice.NextEventNumber;

                foreach (var ev in slice.Events)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("Initialization Cancelled");
                        return null;
                    }

                    try
                    {
                        var de = ev.Event.ToDomainEvent();

                        if (de != null)
                        {
                            var entry = new ProjectionEventData
                            {
                                EventData = de,
                                EventNumber = ev.OriginalEventNumber
                            };

                            await context.HandleEventAsync(entry).ConfigureAwait(false);
                        }
                    }
                    catch (Exception e) when (LogError(e))
                    {
                        _logger.LogError(e, "Error Reading Event: {Stream} {ProjectorId} {OriginalEventNumber}", stream,
                            context.ProjectorId, ev.OriginalEventNumber);
                    }

                }

            } while (!slice.IsEndOfStream);

            _logger.LogDebug("ReadFromStreamAsync {Stream} {ProjectorId} Finished in {Elapsed} ms", stream, context.ProjectorId, sw.ElapsedMilliseconds);

            return context.Status;

        }

        protected bool LogError(Exception ex)
        {
            _logger.LogError(ex, ex.GetBaseException().Message);
            return true;
        }


        private async Task InsertEmptyDomainEventIfStreamIsEmpty(string streamName)
        {
            _logger.LogDebug("InsertEmptyDomainEventIfStreamIsEmpty {StreamName}", streamName);

            var empty = await _connectionProvider.IsStreamEmptyAsync(streamName).ConfigureAwait(false);

            if (empty)
            {
                _logger.LogDebug("Stream is Empty {StreamName}", streamName);

                var events = new[] { DomainEventExtensions.ToEventData(new EmptyDomainEvent(), null) };

                var name = streamName; // $"{aggregateName}-empty";

                //wenn es eine category stream ist, dann den basis stream finden und eine neue gruppe -empty erzeugen
                if (streamName.Contains("$ce-"))
                    name = $"{streamName.Replace("$ce-", string.Empty)}-empty";

                _logger.LogDebug("InsertEmptyEvent: StreamName {StreamName}", name);
                var con = await _connectionProvider.GetActiveConnection().ConfigureAwait(false);
                await con.AppendToStreamAsync(name, -1, events, _connectionProvider.OpsCredentials).ConfigureAwait(false);

            }
            else
            {
                _logger.LogDebug("Stream is not Empty {StreamName}", streamName);
            }

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting InitializeAsync");
            var sw = Stopwatch.StartNew();

            foreach (var projector in _projectors)
            {
                if (string.IsNullOrEmpty(projector.StreamName))
                {
                    throw new InvalidOperationException($"Empty Stream in Projector: {projector.GetType().FullName}");
                }
            }

            var streamsToCheck = _projectors.Select(x => x.StreamName).Distinct().ToArray();

            await Task.WhenAll(streamsToCheck.Select(s =>
            {
                _logger.LogDebug("Preparing Stream {StreamName}", s);
                return InsertEmptyDomainEventIfStreamIsEmpty(s);
            })).ConfigureAwait(false);

          
            await _cache.RemoveByPrefixAsync(CacheKeys.Create<ProjectionInitializationStatus>()).ConfigureAwait(false);
            foreach (var projector in _projectors)
            {
                if (projector != null)
                {
                    var projectorId = projector.GetType().FullName;

                    var statusItem = await _entityStore.LoadAsync<ProjectionInitializationStatus>(projectorId, cancellationToken).ConfigureAwait(false);

                    if (statusItem.IsNew)
                    {
                        var entity = statusItem.Entity;
                        entity.ProjectorId = projectorId;
                        entity.StreamName = projector.StreamName;
                        entity.Status = ProjectionInitializationStatus.StatusNew;
                        entity.ModifiedOnUtc = DateTime.UtcNow;
                        await _entityStore.SaveAsync(statusItem, cancellationToken).ConfigureAwait(false);
                    }
                }
            }


            var contexte = await Task.WhenAll(_projectors.Select(projector =>
            {
                return Policy.Handle<Exception>().RetryAsync(2, (exception, i) =>
                    _logger.LogError(exception, "Retry {Retry}", i)).ExecuteAsync(() => InitProjectorInternal(cancellationToken, projector));
            })).ConfigureAwait(false);

            await Task.WhenAll(contexte.Select(ctx => StartProjectorInternal(cancellationToken, ctx))).ConfigureAwait(false);

            _logger.LogInformation("EventStore Projection Initializer Finished in {Elapsed} ms", sw.ElapsedMilliseconds);
        }


        private async Task StartProjectorInternal(CancellationToken cancellationToken, EventStoreProjectionContext context)
        {
            _logger.LogDebug("Starting Subscription on EventStore for Projector {Projector}", context.ProjectorId);
            await context.StartSubscription(cancellationToken).ConfigureAwait(false);
            await UpdateInitializationStatusAsync(cancellationToken, context.ProjectorId, ProjectionInitializationStatus.StatusReady, "Startet").ConfigureAwait(false);
        }

        private async Task<EventStoreProjectionContext> InitProjectorInternal(CancellationToken cancellationToken, IReadModelProjector projector)
        {
            var projectorId = projector.GetType().FullName;
            projector.Initialize(cancellationToken);

            _logger.LogInformation("Initialize Projector {ProjectorName}", projectorId);

            await UpdateInitializationStatusAsync(cancellationToken, projectorId, ProjectionInitializationStatus.StatusPending, "Starting").ConfigureAwait(false);

            var context =
                new EventStoreProjectionContext(_loggerFactory, _connectionProvider, _entityStore, projector, _options.UseQueue, projectorId);

            projector.Context = context;

            //   await _cache.RemoveByPrefixAsync1(CacheKeys.Create<ProjectionStatus>());
            var status = await _entityStore.LoadAsync<ProjectionStatus>(projectorId, cancellationToken).ConfigureAwait(false);
            context.Status = status.Entity;

            if (status.IsNew)
            {
                _logger.LogDebug("Starting Empty Initialization for Projector {Projector}", projectorId);

                await UpdateInitializationStatusAsync(cancellationToken, projectorId, ProjectionInitializationStatus.StatusPending, "StartingInitialization").ConfigureAwait(false);

                await context.StartInitializationModeAsync().ConfigureAwait(false);

                await UpdateInitializationStatusAsync(cancellationToken, projectorId, ProjectionInitializationStatus.StatusPending, "InitializationStartingStreamReading").ConfigureAwait(false);

                try
                {
                    var tempStatus = await ReadStreamAsync(context.StreamName, context, cancellationToken).ConfigureAwait(false);

                    if (tempStatus != null)
                    {
                        status.Entity.LastCheckPoint = tempStatus.LastCheckPoint;
                        status.Entity.ModifiedOnUtc = tempStatus.ModifiedOnUtc;
                    }
                }
                catch (Exception ex) when (LogError(ex))
                {
                    _logger.LogError(ex, "Error ReadStreamAsync");
                    throw;
                }

                await UpdateInitializationStatusAsync(cancellationToken, projectorId, ProjectionInitializationStatus.StatusPending, "InitializationStartingCopy").ConfigureAwait(false);
                await context.UpdateStreamStatusAsync().ConfigureAwait(false);

                try
                {
                    await projector.CopyEntitiesAsync(context.EntityStore, _entityStore, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (LogError(ex))
                {
                    _logger.LogError(ex, "Error CopyEntitites");
                    throw;
                }

                await context.StopInitializationModeAsync().ConfigureAwait(false);
                await _entityStore.SaveAsync(status, cancellationToken).ConfigureAwait(false);

                await UpdateInitializationStatusAsync(cancellationToken, projectorId, ProjectionInitializationStatus.StatusPending, "InitializationFinished").ConfigureAwait(false);

            }

            cancellationToken.ThrowIfCancellationRequested();
            return context;
        }

        private async Task UpdateInitializationStatusAsync(CancellationToken cancellationToken, string projectorId, string status, string statusText)
        {
            cancellationToken.ThrowIfCancellationRequested();
            //     await _cache.RemoveByPrefixAsync(CacheKeys.Create<ProjectionInitializationStatus>());
            var statusItem = await _entityStore.LoadAsync<ProjectionInitializationStatus>(projectorId, cancellationToken).ConfigureAwait(false);
            statusItem.Entity.Status = status;
            statusItem.Entity.StatusText = statusText;
            await _entityStore.SaveAsync(statusItem, cancellationToken).ConfigureAwait(false);
        }


        public async Task<ProjectionRegistryStatus> GetStatusAsync()
        {
            _logger.LogTrace("GetStatusAsync");

            var states = await _entityStore.LoadItemsAsync<ProjectionInitializationStatus>().ConfigureAwait(false);
            var enumerable = states as ProjectionInitializationStatus[] ?? states.ToArray();
            var temp = new ProjectionRegistryStatus()
            {
                Projectors = enumerable,
                Status = ProjectionInitializationStatus.StatusNew
            };


            if (!enumerable.Any())
            {
                return temp;
            }

            if (enumerable.All(x => x.Status == ProjectionInitializationStatus.StatusReady))
            {
                temp.Status = ProjectionInitializationStatus.StatusReady;
            }
            else
            {
                if (enumerable.Any(x => x.Status == ProjectionInitializationStatus.StatusPending))
                {
                    temp.Status = ProjectionInitializationStatus.StatusPending;
                }
                else
                {
                    temp.Status = ProjectionInitializationStatus.StatusNew;
                }
            }

            temp.Pending = temp.Projectors.Count(x => x.Status == ProjectionInitializationStatus.StatusPending);
            temp.Ready = temp.Projectors.Count(x => x.Status == ProjectionInitializationStatus.StatusReady);
            temp.Total = temp.Projectors.Length;

            return temp;
        }


    }
}
