using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.DomainModel
{
    /// <summary>
    /// Speichern der Events eines Aggregates 
    /// Aggregate Laden und Zustand herstellen
    /// </summary>    
    public interface IRepository
    {


        /// <summary>
        /// Speichern der änderungen, ohne Prüfung auf die erwartete Version
        /// </summary>

        Task SaveAsync<T>(T aggregate, CancellationToken token = default(CancellationToken),
            IDictionary<string, string> state = null) where T : AggregateRoot;


        Task SaveAsync<T>(T aggregate, int expectedVersion, CancellationToken token = default(CancellationToken),
            IDictionary<string, string> state = null) where T : AggregateRoot;

        /// <summary>
        /// Prüft, ob das Aggregate existiert
        /// </summary>
        /// <returns></returns>
        Task<bool> CheckAggregateExists<T>(string aggregateId, CancellationToken token = default(CancellationToken),
            IDictionary<string, string> state = null) where T : AggregateRoot;

        /// <summary>
        /// Prüft, ob das Aggregate existiert aus einem custom Stream
        /// </summary>
        /// <returns></returns>
        Task<bool> CheckAggregateExists<T>(string aggregateId, string streamName, CancellationToken token = default(CancellationToken),
            IDictionary<string, string> state = null) where T : AggregateRoot;

        /// <summary>
        /// Laden aller Events und Erzeugung des Aggregats
        /// </summary>
        /// <returns></returns>
        Task<T> GetByIdAsync<T>(string id, CancellationToken token = default(CancellationToken),
            IDictionary<string, string> state = null) where T : AggregateRoot, new();

        /// <summary>
        /// Ladern aller Events und Erzeugung des Aggregats
        /// es wird geprüft, ob die letzte Event Version übereinstimmt
        /// </summary>
        /// <returns></returns>
        Task<T> GetByIdAsync<T>(string id, int expectedAggregateVersion, CancellationToken token = default(CancellationToken),
            IDictionary<string, string> state = null) where T : AggregateRoot, new();

        /// <summary>
        /// Laden aller Events und Erzeugung des Aggregats aus einem eigenen Stream
        /// </summary>
        /// <returns></returns>
        Task<T> GetByIdAsync<T>(string id, string streamName, CancellationToken token = default(CancellationToken),
            IDictionary<string, string> state = null) where T : AggregateRoot, new();

        /// <summary>
        /// Ladern aller Events und Erzeugung des Aggregats aus einem eigenen Stream
        /// es wird geprüft, ob die letzte Event Version übereinstimmt
        /// </summary>
        /// <returns></returns>
        Task<T> GetByIdAsync<T>(string id, int expectedAggregateVersion, string streamName, CancellationToken token = default(CancellationToken),
            IDictionary<string, string> state = null) where T : AggregateRoot, new();

    }


}