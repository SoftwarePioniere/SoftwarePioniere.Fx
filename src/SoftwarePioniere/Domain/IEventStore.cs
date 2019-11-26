using System.Collections.Generic;
using System.Threading.Tasks;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Domain
{
    /// <summary>
    ///     Laden und speichern von Events
    /// </summary>
    public interface IEventStore
    {
       
        Task<IList<EventDescriptor>> GetEventsForAggregateAsync<T>(string aggregateId) where T : AggregateRoot;
        
        //Task<IList<EventDescriptor>> GetEventsForAggregateAsync<T>(string aggregateId, string streamName) where T : AggregateRoot;
        
        Task<bool> CheckAggregateExists<T>(string aggregateId) where T : AggregateRoot;

        //Task<bool> CheckAggregateExists<T>(string aggregateId, string streamName) where T : AggregateRoot;
        
        Task SaveEventsAsync<T>(string aggregateId, IEnumerable<IDomainEvent> events, int aggregateVersion) where T : AggregateRoot;
    }    
}