using System.Threading.Tasks;

namespace SoftwarePioniere.Domain
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