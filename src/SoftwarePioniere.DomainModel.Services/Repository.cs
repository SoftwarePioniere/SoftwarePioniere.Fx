using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.DomainModel.Exceptions;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.Services
{
    public class RepositoryOptions
    {
        public bool SendInternalEvents { get; set; }
    }

    public class Repository : IRepository
    {
        private readonly ILogger _logger;
        private readonly IEventStore _store;
        private readonly IMessageBusAdapter _publisher;
        private readonly RepositoryOptions _options;

        public Repository(ILoggerFactory loggerFactory, IEventStore store, IMessageBusAdapter bus, IOptions<RepositoryOptions> options)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(GetType());
            _store = store ?? throw new ArgumentNullException(nameof(store));

            _publisher = bus ?? throw new ArgumentNullException(nameof(bus));

            _options = options.Value;
        }

        public virtual async Task SaveAsync<T>(T aggregate, int expectedVersion, CancellationToken token = default(CancellationToken)
            //,IDictionary<string, string> state = null
            ) where T : AggregateRoot
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

                //var aggregateName = typeof(T).GetAggregateName();

                foreach (var @event in events)
                {
                    token.ThrowIfCancellationRequested();

                    if (_options.SendInternalEvents)
                    {
                        _logger.LogTrace("SaveAsync: PublishMessageAsync {@Message}", @event);
                        await _publisher.PublishAsync(@event.GetType(), @event, TimeSpan.Zero, token)//, state)
                            .ConfigureAwait(false);
                    }

                    {

                        //var typeArgument1 = typeof(T);
                        //var typeArgument2 = @event.GetType();

                        //var genericClass = typeof(AggregateDomainEventMessage<,>);
                        //var constructedClass = genericClass.MakeGenericType(typeArgument1, typeArgument2);

                        //// public AggregateDomainEventMessage(Guid id, DateTime timeStampUtc, string userId, TDomainEvent domainEventContent, string aggregateId) : base(id, timeStampUtc, userId)                        
                        //var created = Activator.CreateInstance(constructedClass,
                        //    Guid.NewGuid(), @event.TimeStampUtc, @event.UserId,
                        //    @event, aggregate.Id);

                        var created = @event.CreateAggregateDomainEventMessage(aggregate);

                        _logger.LogTrace("SaveAsync: Publish AggregateDomainEventMessage {@Message}", created);
                        await _publisher.PublishAsync(created.GetType(), created, TimeSpan.Zero, token)//, state)
                            .ConfigureAwait(false);
                    }

                }
            }
        }

        public async Task SaveAsync<T>(T aggregate, CancellationToken token = default(CancellationToken)
            //,IDictionary<string, string> state = null
            ) where T : AggregateRoot
        {
            //var parentState = new Dictionary<string, string>();

            //if (state != null)
            //{
            //    foreach (var key in state.Keys)
            //    {
            //        parentState.Add(key, state[key]);
            //    }
            //}

            token.ThrowIfCancellationRequested();
            await SaveAsync(aggregate, aggregate.Version, token).ConfigureAwait(false);
        }

        public virtual Task<bool> CheckAggregateExists<T>(string aggregateId, CancellationToken token = default(CancellationToken)
            //,IDictionary<string, string> state = null
            ) where T : AggregateRoot
        {
            token.ThrowIfCancellationRequested();
            return _store.CheckAggregateExists<T>(aggregateId);
        }

        public async Task<T> GetByIdAsync<T>(string id, CancellationToken token = default(CancellationToken)
            //,IDictionary<string, string> state = null
            ) where T : AggregateRoot, new()
        {
            var agg = await GetByIdAsync<T>(id, -1, token).ConfigureAwait(false);
            return agg;
        }

        public virtual async Task<T> GetByIdAsync<T>(string id, int expectedAggregateVersion, CancellationToken token = default(CancellationToken)
            //,IDictionary<string, string> state = null
            ) where T : AggregateRoot, new()
        {
            _logger.LogDebug("GetByIdAsync {Type} {AggregateId} and {ExcpectedVersion}", typeof(T), id, expectedAggregateVersion);
            token.ThrowIfCancellationRequested();

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

        public async Task<T> GetByIdAsync<T>(string id, string streamName, CancellationToken token = default
            //,IDictionary<string, string> state = null
            ) where T : AggregateRoot, new()
        {
            _logger.LogDebug("GetByIdAsync {Type} {AggregateId} {StreamName}", typeof(T), id, streamName);

            var agg = await GetByIdAsync<T>(id, -1, streamName, token//, state
                ).ConfigureAwait(false);
            return agg;
        }

        public virtual async Task<T> GetByIdAsync<T>(string id, int expectedAggregateVersion, string streamName, CancellationToken token = default
            //,IDictionary<string, string> state = null
            ) where T : AggregateRoot, new()
        {
            _logger.LogDebug("GetByIdAsync {Type} {AggregateId} and {ExcpectedVersion}  {StreamName}", typeof(T), id, expectedAggregateVersion, streamName);
            token.ThrowIfCancellationRequested();

            var aggregate = Activator.CreateInstance<T>();
            aggregate.SetId(id);
            var eventDescriptors = await _store.GetEventsForAggregateAsync<T>(id, streamName).ConfigureAwait(false);
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

        public virtual Task<bool> CheckAggregateExists<T>(string aggregateId, string streamName, CancellationToken token = default
            //,IDictionary<string, string> state = null
            ) where T : AggregateRoot
        {
            throw new NotImplementedException();
        }
    }
}