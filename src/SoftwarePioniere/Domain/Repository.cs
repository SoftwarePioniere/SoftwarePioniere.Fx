using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Domain.Exceptions;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Domain
{
    public class Repository : IRepository
    {
        private readonly ILogger _logger;
        private readonly RepositoryOptions _options;
        private readonly IMessageBusAdapter _publisher;
        private readonly IEventStore _store;

        public Repository(ILoggerFactory loggerFactory, IEventStore store, IMessageBusAdapter bus, IOptions<RepositoryOptions> options)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(GetType());
            _store = store ?? throw new ArgumentNullException(nameof(store));

            _publisher = bus ?? throw new ArgumentNullException(nameof(bus));

            _options = options.Value;
        }

        public virtual async Task SaveAsync<T>(T aggregate, int expectedVersion, CancellationToken token = default) where T : AggregateRoot
        {
            _logger.LogDebug("SaveAsync {Type} {AggregateId} {ExpectedVersion}", typeof(T), expectedVersion, aggregate.Id);

            var events = aggregate.GetUncommittedChanges().ToArray();

            if (events.Length == 0)
            {
                _logger.LogTrace("SaveAsync {Type} {AggregateId} {ExpectedVersion} No Events", typeof(T), expectedVersion, aggregate.Id);
            }
            else
            {
                token.ThrowIfCancellationRequested();

                _logger.LogAggregate(aggregate);

                var sw = Stopwatch.StartNew();
                await _store.SaveEventsAsync<T>(aggregate.Id, events, expectedVersion).ConfigureAwait(false);
                aggregate.MarkChangesAsCommitted();

                sw.Stop();
                _logger.LogDebug("SaveAsync Events Saved {Type} {AggregateId} in {Elapsed:0.0000} ms ", typeof(T), expectedVersion, sw.ElapsedMilliseconds);

                foreach (var @event in events)
                {
                    token.ThrowIfCancellationRequested();

                    if (_options.SendInternalEvents)
                    {
                        _logger.LogTrace("SaveAsync: PublishMessageAsync {@Message}", @event);
                        await _publisher.PublishAsync(@event.GetType(), @event, TimeSpan.Zero, token)
                            .ConfigureAwait(false);
                    }

                    {
                      
                        var created = @event.CreateAggregateDomainEventMessage(aggregate);

                        _logger.LogTrace("SaveAsync: Publish AggregateDomainEventMessage {@Message}", created);
                        await _publisher.PublishAsync(created.GetType(), created, TimeSpan.Zero, token).ConfigureAwait(false);
                    }
                }
            }
        }

        public async Task SaveAsync<T>(T aggregate, CancellationToken token = default) where T : AggregateRoot
        {
         
            token.ThrowIfCancellationRequested();
            await SaveAsync(aggregate, aggregate.Version, token).ConfigureAwait(false);
        }

        public virtual Task<bool> CheckAggregateExists<T>(string aggregateId, CancellationToken token = default) where T : AggregateRoot
        {
            token.ThrowIfCancellationRequested();
            return _store.CheckAggregateExists<T>(aggregateId);
        }

       
        public async Task<T> GetByIdAsync<T>(string id, CancellationToken token = default) where T : AggregateRoot, new()
        {
            var agg = await GetByIdAsync<T>(id, -1, token).ConfigureAwait(false);
            return agg;
        }

        public virtual async Task<T> GetByIdAsync<T>(string id, int expectedAggregateVersion, CancellationToken token = default) where T : AggregateRoot, new()
        {
            _logger.LogDebug("GetByIdAsync {Type} {AggregateId} and {ExcpectedVersion}", typeof(T), id, expectedAggregateVersion);
            token.ThrowIfCancellationRequested();

            var aggregate = Activator.CreateInstance<T>();
            aggregate.SetId(id);
            var eventDescriptors = await _store.GetEventsForAggregateAsync<T>(id).ConfigureAwait(false);
            aggregate.LoadFromHistory(eventDescriptors);

            if (expectedAggregateVersion != -1 && expectedAggregateVersion != aggregate.Version)
                throw new ConcurrencyException
                {
                    ExpectedVersion = expectedAggregateVersion,
                    CurrentVersion = aggregate.Version,
                    AggregateType = typeof(T)
                };

            return aggregate;
        }

        public async Task<T> GetByIdAsync<T>(string id, string streamName, CancellationToken token = default) where T : AggregateRoot, new()
        {
            _logger.LogDebug("GetByIdAsync {Type} {AggregateId} {StreamName}", typeof(T), id, streamName);

            var agg = await GetByIdAsync<T>(id, -1, streamName, token).ConfigureAwait(false);
            return agg;
        }

        public virtual async Task<T> GetByIdAsync<T>(string id, int expectedAggregateVersion, string streamName, CancellationToken token = default) where T : AggregateRoot, new()
        {
            _logger.LogDebug("GetByIdAsync {Type} {AggregateId} and {ExcpectedVersion}  {StreamName}", typeof(T), id, expectedAggregateVersion, streamName);
            token.ThrowIfCancellationRequested();

            var aggregate = Activator.CreateInstance<T>();
            aggregate.SetId(id);
            var eventDescriptors = await _store.GetEventsForAggregateAsync<T>(id).ConfigureAwait(false);
            aggregate.LoadFromHistory(eventDescriptors);

            if (expectedAggregateVersion != -1 && expectedAggregateVersion != aggregate.Version)
                throw new ConcurrencyException
                {
                    ExpectedVersion = expectedAggregateVersion,
                    CurrentVersion = aggregate.Version,
                    AggregateType = typeof(T)
                };

            return aggregate;
        }
    }
}