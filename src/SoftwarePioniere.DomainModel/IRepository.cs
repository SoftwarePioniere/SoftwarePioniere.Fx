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
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregate"></param>
        Task SaveAsync<T>(T aggregate) where T : AggregateRoot;

        Task SaveAsync<T>(T aggregate, int expectedVersion) where T : AggregateRoot;

        /// <summary>
        /// Prüft, ob das Aggregate existiert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregateId"></param>
        /// <returns></returns>
        Task<bool> CheckAggregateExists<T>(string aggregateId) where T : AggregateRoot;

        /// <summary>
        /// Laden aller Events und Erzeugung des Aggregats
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> GetByIdAsync<T>(string id) where T : AggregateRoot, new();

        /// <summary>
        /// Ladern aller Events und Erzeugung des Aggregats
        /// es wird geprüft, ob die letzte Event Version übereinstimmt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="expectedAggregateVersion"></param>
        /// <returns></returns>
         Task<T> GetByIdAsync<T>(string id, int expectedAggregateVersion) where T : AggregateRoot, new();
    }


}