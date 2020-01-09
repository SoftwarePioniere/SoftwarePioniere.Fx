using System;
using System.Collections.Generic;
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

            var state = new Dictionary<string, object>
            {
                {"AggregateId", aggregate.AggregateId},
                {"ExpectedVersion",expectedVersion},
                {"Type", typeof(T).FullName}
            };

            using (_logger.BeginScope(state))
            {
                var sw = Stopwatch.StartNew();
                _logger.LogDebug("SaveAsync {Type} {AggregateId} {ExpectedVersion} started", typeof(T), expectedVersion, aggregate.AggregateId);

                var events = aggregate.GetUncommittedChanges().ToArray();

                if (events.Length == 0)
                {
                    _logger.LogTrace("SaveAsync {Type} {AggregateId} {ExpectedVersion} No Events", typeof(T), expectedVersion, aggregate.AggregateId);
                }
                else
                {
                    token.ThrowIfCancellationRequested();

                    _logger.LogAggregate(aggregate);

                    await _store.SaveEventsAsync<T>(aggregate.AggregateId, events, expectedVersion).ConfigureAwait(false);
                    aggregate.MarkChangesAsCommitted();

                    var tasks = new List<Task>();

                    foreach (var @event in events)
                    {
                        token.ThrowIfCancellationRequested();

                        if (_options.SendInternalEvents)
                        {
                            _logger.LogTrace("SaveAsync: PublishMessageAsync {@Message}", @event);
                            tasks.Add(_publisher.PublishAsync(@event.GetType(), @event, TimeSpan.Zero, token));
                        }

                        {

                            var created = @event.CreateAggregateDomainEventMessage(aggregate);

                            _logger.LogTrace("SaveAsync: Publish AggregateDomainEventMessage {@Message}", created);
                            tasks.Add(_publisher.PublishAsync(created.GetType(), created, TimeSpan.Zero, token));
                        }
                    }

                    await Task.WhenAll(tasks);
                }
                sw.Stop();
                _logger.LogDebug("SaveAsync {Type} {AggregateId} finished in {Elapsed} ms ", typeof(T), expectedVersion, sw.ElapsedMilliseconds);

            }
        }

        public async Task<IEnumerable<AggregateDomainEventMessage>> SaveAsyncWithOutPush<T>(T aggregate, CancellationToken token = default) where T : AggregateRoot
        {
            token.ThrowIfCancellationRequested();
            return await SaveAsyncWithOutPush(aggregate, aggregate.Version, token).ConfigureAwait(false);
        }

        public async Task<IEnumerable<AggregateDomainEventMessage>> SaveAsyncWithOutPush<T>(T aggregate, int expectedVersion, CancellationToken token = default) where T : AggregateRoot
        {

            var state = new Dictionary<string, object>
            {
                {"AggregateId", aggregate.AggregateId},
                {"ExpectedVersion",expectedVersion},
                {"Type", typeof(T).FullName}
            };

            using (_logger.BeginScope(state))
            {
                var result = new List<AggregateDomainEventMessage>();

                var sw = Stopwatch.StartNew();
                _logger.LogDebug("SaveAsync {Type} {AggregateId} {ExpectedVersion} started", typeof(T), expectedVersion, aggregate.AggregateId);

                var events = aggregate.GetUncommittedChanges().ToArray();

                if (events.Length == 0)
                {
                    _logger.LogTrace("SaveAsync {Type} {AggregateId} {ExpectedVersion} No Events", typeof(T), expectedVersion, aggregate.AggregateId);
                }
                else
                {
                    token.ThrowIfCancellationRequested();

                    _logger.LogAggregate(aggregate);

                    await _store.SaveEventsAsync<T>(aggregate.AggregateId, events, expectedVersion).ConfigureAwait(false);
                    aggregate.MarkChangesAsCommitted();



                    foreach (var @event in events)
                    {
                        token.ThrowIfCancellationRequested();

                        //if (_options.SendInternalEvents)
                        //{
                        //    _logger.LogTrace("SaveAsync: PublishMessageAsync {@Message}", @event);
                        //    await _publisher.PublishAsync(@event.GetType(), @event, TimeSpan.Zero, token)
                        //        .ConfigureAwait(false);
                        //}

                        {

                            var created = @event.CreateAggregateDomainEventMessage(aggregate);
                            result.Add(created);

                            //_logger.LogTrace("SaveAsync: Publish AggregateDomainEventMessage {@Message}", created);
                            //await _publisher.PublishAsync(created.GetType(), created, TimeSpan.Zero, token).ConfigureAwait(false);
                        }
                    }
                }
                sw.Stop();
                _logger.LogDebug("SaveAsync {Type} {AggregateId} finished in {Elapsed} ms ", typeof(T), expectedVersion, sw.ElapsedMilliseconds);

                return result;
            }


        }

        public async Task SaveAsync<T>(T aggregate, CancellationToken token = default) where T : AggregateRoot
        {

            token.ThrowIfCancellationRequested();
            await SaveAsync(aggregate, aggregate.Version, token).ConfigureAwait(false);
        }

        public virtual async Task<bool> CheckAggregateExists<T>(string aggregateId, CancellationToken token = default) where T : AggregateRoot
        {
            var state = new Dictionary<string, object>
            {
                {"AggregateId", aggregateId},
                {"Type", typeof(T).FullName}
            };

            using (_logger.BeginScope(state))
            {
                var sw = Stopwatch.StartNew();

                _logger.LogDebug("CheckAggregateExists started");

                token.ThrowIfCancellationRequested();
                var result = await _store.CheckAggregateExists<T>(aggregateId);

                sw.Stop();

                _logger.LogDebug("CheckAggregateExists  finished in {Elapsed} ms ", sw.ElapsedMilliseconds);

                return result;
            }
        }


        public async Task<T> GetByIdAsync<T>(string aggregateId, CancellationToken token = default) where T : AggregateRoot, new()
        {
            var agg = await GetByIdAsync<T>(aggregateId, -1, token).ConfigureAwait(false);
            return agg;
        }

        public virtual async Task<T> GetByIdAsync<T>(string aggregateId, int expectedVersion, CancellationToken token = default) where T : AggregateRoot, new()
        {
            var state = new Dictionary<string, object>
            {
                {"AggregateId", aggregateId},
                {"ExpectedVersion",expectedVersion},
                {"Type", typeof(T).FullName}
            };

            using (_logger.BeginScope(state))
            {
                var sw = Stopwatch.StartNew();

                _logger.LogDebug("GetByIdAsync started");

                _logger.LogDebug("GetByIdAsync {Type} {AggregateId} and {ExcpectedVersion}", typeof(T), aggregateId, expectedVersion);
                token.ThrowIfCancellationRequested();

                var aggregate = Activator.CreateInstance<T>();
                aggregate.SetAggregateId(aggregateId);
                var eventDescriptors = await _store.GetEventsForAggregateAsync<T>(aggregateId).ConfigureAwait(false);
                aggregate.LoadFromHistory(eventDescriptors);

                if (expectedVersion != -1 && expectedVersion != aggregate.Version)
                    throw new ConcurrencyException
                    {
                        ExpectedVersion = expectedVersion,
                        CurrentVersion = aggregate.Version,
                        AggregateType = typeof(T)
                    };

                sw.Stop();

                _logger.LogDebug("GetByIdAsync finished in {Elapsed} ms ", sw.ElapsedMilliseconds);


                return aggregate;

            }
        }

        public async Task<T> GetByIdAsync<T>(string aggregateId, string streamName, CancellationToken token = default) where T : AggregateRoot, new()
        {
            _logger.LogDebug("GetByIdAsync {Type} {AggregateId} {StreamName}", typeof(T), aggregateId, streamName);

            var agg = await GetByIdAsync<T>(aggregateId, -1, streamName, token).ConfigureAwait(false);
            return agg;
        }

        public virtual async Task<T> GetByIdAsync<T>(string aggregateId, int expectedVersion, string streamName, CancellationToken token = default) where T : AggregateRoot, new()
        {
            var state = new Dictionary<string, object>
            {
                {"AggregateId", aggregateId},
                {"ExpectedVersion",expectedVersion},
                {"Type", typeof(T).FullName}
            };

            using (_logger.BeginScope(state))
            {
                var sw = Stopwatch.StartNew();

                _logger.LogDebug("GetByIdAsync started");

                _logger.LogDebug("GetByIdAsync {Type} {AggregateId} and {ExcpectedVersion}  {StreamName}", typeof(T), aggregateId, expectedVersion, streamName);
                token.ThrowIfCancellationRequested();

                var aggregate = Activator.CreateInstance<T>();
                aggregate.SetAggregateId(aggregateId);
                var eventDescriptors = await _store.GetEventsForAggregateAsync<T>(aggregateId).ConfigureAwait(false);
                aggregate.LoadFromHistory(eventDescriptors);

                if (expectedVersion != -1 && expectedVersion != aggregate.Version)
                    throw new ConcurrencyException
                    {
                        ExpectedVersion = expectedVersion,
                        CurrentVersion = aggregate.Version,
                        AggregateType = typeof(T)
                    };

                sw.Stop();

                _logger.LogDebug("GetByIdAsync finished in {Elapsed} ms ", sw.ElapsedMilliseconds);

                return aggregate;

            }
        }
    }
}