using System;
using System.Linq;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.DomainModel.Exceptions;

namespace SoftwarePioniere.DomainModel.Services
{
    public class Repository : IRepository
    {
        private readonly ILogger _logger;
        private readonly IEventStore _store;
        private readonly IMessagePublisher _publisher;

        public Repository(ILoggerFactory loggerFactory, IEventStore store, IMessagePublisher bus)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(GetType());
            _store = store ?? throw new ArgumentNullException(nameof(store));

            _publisher = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task SaveAsync<T>(T aggregate, int expectedVersion) where T : AggregateRoot
        {
            _logger.LogDebug("SaveAsync {Type} {AggregateId} {ExpectedVersion}", typeof(T), expectedVersion, aggregate.Id);

            var events = aggregate.GetUncommittedChanges().ToArray();

            if (events.Length == 0)
            {
                _logger.LogDebug("SaveAsync {Type} {AggregateId} {ExpectedVersion} No Events", typeof(T), expectedVersion, aggregate.Id);
            }
            else
            {

                await _store.SaveEventsAsync<T>(aggregate.Id, events, expectedVersion).ConfigureAwait(false);
                aggregate.MarkChangesAsCommitted();

                var aggregateName = typeof(T).GetAggregateName();

                foreach (var @event in events)
                {

                    _logger.LogDebug("SaveAsync: PublishMessageAsync {@Message}", @event);
                    await _publisher.PublishAsync(@event.GetType(), @event).ConfigureAwait(false);

                    _logger.LogDebug("SaveAsync: CreateDomainEventMessage {EventType}", @event.GetType());
                    var idem = @event.CreateDomainEventMessageFromType(aggregateName, aggregate.Id);

                    _logger.LogDebug("SaveAsync: Publish DomainEventMessage {@Message}", idem);
                    await _publisher.PublishAsync(idem.GetType(), idem).ConfigureAwait(false);

                }
            }
        }

        public async Task SaveAsync<T>(T aggregate) where T : AggregateRoot
        {
            _logger.LogDebug("SaveAsync {AggregateType} {AggregateId} {AggregateVersion}", typeof(T), aggregate.Id, aggregate.Version);
            await SaveAsync(aggregate, aggregate.Version).ConfigureAwait(false);
        }

        public Task<bool> CheckAggregateExists<T>(string aggregateId) where T : AggregateRoot
        {
            return _store.CheckAggregateExists<T>(aggregateId);
        }

        public async Task<T> GetByIdAsync<T>(string id) where T : AggregateRoot, new()
        {
            _logger.LogDebug("GetByIdAsync {Type} {AggregateId}", typeof(T), id);

            var agg = await GetByIdAsync<T>(id, -1).ConfigureAwait(false);
            return agg;
        }

        public async Task<T> GetByIdAsync<T>(string id, int expectedAggregateVersion) where T : AggregateRoot, new()
        {
            _logger.LogDebug("GetByIdAsync {Type} {AggregateId} and {ExcpectedVersion}", typeof(T), id, expectedAggregateVersion);

            var aggregate = Activator.CreateInstance<T>();
            aggregate.SetId(id);
            var eventDescriptors = await _store.GetEventsForAggregateAsync<T>(id).ConfigureAwait(false);
            aggregate.LoadFromHistory(eventDescriptors);

            if (expectedAggregateVersion != -1 && expectedAggregateVersion != aggregate.Version)
            {
                throw new ConcurrencyException
                {
                    ExpectedVersion = expectedAggregateVersion,
                    CurrentVersion = aggregate.Version,
                    AggregateType = typeof(T)
                };

            }

            return aggregate;
        }
    }
}