﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Domain
{
    /// <summary>
    ///     Laden und speichern von Events
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        ///     Alle Events eines Aggregats auslesen
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <returns></returns>
        Task<IList<EventDescriptor>> GetEventsForAggregateAsync<T>(string aggregateId) where T : AggregateRoot;


        Task<IList<EventDescriptor>> GetEventsForAggregateAsync<T>(string aggregateId, string streamName) where T : AggregateRoot;

        /// <summary>
        /// Prüft, ob das Aggregate existiert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregateId"></param>
        /// <returns></returns>
        Task<bool> CheckAggregateExists<T>(string aggregateId) where T : AggregateRoot;

        Task<bool> CheckAggregateExists<T>(string aggregateId, string streamName) where T : AggregateRoot;

        /// <summary>
        ///     Speichert Events und prüft ggf auch auf die EventVersion
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <param name="events"></param>
        /// <param name="aggregateVersion">aktuelle Version im Aggregate</param>
        Task SaveEventsAsync<T>(string aggregateId, IEnumerable<IDomainEvent> events, int aggregateVersion) where T : AggregateRoot;
    }    
}