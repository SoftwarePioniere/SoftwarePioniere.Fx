using System.Threading.Tasks;

namespace SoftwarePioniere.DomainModel
{
    public interface IEventStoreTests
    {
        Task CheckAggregateExists();

        Task SaveAndLoadContainsAllEventsForAnAggregate();

        void LoadThrowsErrorIfAggregateWithIdNotFound();

        Task SaveThrowsErrorIfVersionsNotMatch();

        Task SavesEventsWithExpectedVersion();
        
    }
}
