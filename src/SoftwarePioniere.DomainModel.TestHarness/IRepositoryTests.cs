using System.Threading.Tasks;

namespace SoftwarePioniere.DomainModel
{
    public interface IRepositoryTests
    {
        Task SaveCallsEventStoreSavingAsync();

        Task EventsWillBePushblishedAfterSavingAsync();

        Task LoadCreatesAggregateAsync();
    }
}