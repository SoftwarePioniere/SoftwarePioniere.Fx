using System.Threading.Tasks;

namespace SoftwarePioniere.DomainModel
{
    public interface IRepositoryTests
    {
        void SaveWithCancelationThrowsError();

        Task SaveCallsEventStoreSavingAsync();

        Task EventsWillBePushblishedAfterSavingAsync();

        Task LoadCreatesAggregateAsync();

        void LoadWithCancelationThrowsError();

        void CheckExistsWithCancelationThrowsError();


    }
}