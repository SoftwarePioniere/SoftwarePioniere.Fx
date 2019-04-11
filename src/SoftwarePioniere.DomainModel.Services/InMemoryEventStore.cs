using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.DomainModel.Exceptions;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel
{
    /// <summary>
    ///     In Memory Event Store
    /// </summary>
    public class InMemoryEventStore : IEventStore
    {
        private readonly Dictionary<string, IList<EventDescriptor>> _current = new Dictionary<string, IList<EventDescriptor>>();

        private readonly ILogger _logger;

        public InMemoryEventStore(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public Task<IList<EventDescriptor>> GetEventsForAggregateAsync<T>(string aggregateId) where T : AggregateRoot
        {
            // collect all processed events for given aggregate and return them as a list
            // used to build up an aggregate from its history (Domain.LoadsFromHistory)

            if (!_current.TryGetValue(aggregateId, out var eventDescriptors))
                throw new AggregateNotFoundException(aggregateId, typeof(T));

            return Task.FromResult(eventDescriptors);
        }

        public Task<IList<EventDescriptor>> GetEventsForAggregateAsync<T>(string aggregateId, string streamName) where T : AggregateRoot
        {
            return GetEventsForAggregateAsync<T>(aggregateId);
        }


        public Task<bool> CheckAggregateExists<T>(string aggregateId) where T : AggregateRoot
        {
            return Task.FromResult(_current.ContainsKey(aggregateId));
        }

        public Task SaveEventsAsync<T>(string aggregateId, IEnumerable<IDomainEvent> events, int aggregateVersion) where T : AggregateRoot
        {
            _logger.LogTrace("SaveEvents {Type} {AggregateId} {AggregateVersion}", typeof(T), aggregateId, aggregateVersion);

            return Task.Run(() => { SaveInternal<T>(aggregateId, events, aggregateVersion); });
        }

        private void SaveInternal<T>(string aggregateId, IEnumerable<IDomainEvent> events, int aggregateVersion) where T : AggregateRoot
        {
            var domainEvents = events as IDomainEvent[] ?? events.ToArray();

            // die version, die das aggregate vor den neuen events hatte
            var originalVersion = aggregateVersion - domainEvents.Length;
            //das ist die nummer, bei der der stream jetzt stehen sollte, so wird sichergestellt, dass in der zwischnzeit keine events geschrieben wurden
            var expectedVersion = originalVersion;


            // try to get event descriptors list for given aggregate id
            // otherwise -> create empty dictionary
            if (!_current.TryGetValue(aggregateId, out var eventDescriptors))
            {
                eventDescriptors = new List<EventDescriptor>();
                _current.Add(aggregateId, eventDescriptors);
            }
            else
            {

                if (expectedVersion != -1 && eventDescriptors.Last().Version != expectedVersion)
                {
                    throw new ConcurrencyException
                    {
                        ExpectedVersion = aggregateVersion,
                        CurrentVersion = eventDescriptors[eventDescriptors.Count - 1].Version,
                        AggregateType = typeof(T)
                    };
                }
            }

            var i = expectedVersion;

            // iterate through current aggregate events increasing version with each processed event
            foreach (var @event in domainEvents)
            {
                i++;

                // push event to the event descriptors list for current aggregate
                eventDescriptors.Add(new EventDescriptor(@event, i));

                //@event.EventVersion = i;
                //// push event to the event descriptors list for current aggregate
                //_current.Add(@event);
            }
        }


        public Task<bool> CheckAggregateExists<T>(string aggregateId, string streamName) where T : AggregateRoot
        {
            return CheckAggregateExists<T>(aggregateId);
        }
    }
}