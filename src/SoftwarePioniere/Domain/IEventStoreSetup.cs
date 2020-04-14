using System.Threading.Tasks;

namespace SoftwarePioniere.Domain
{
    public interface IEventStoreSetup
    {
        Task AddOpsUserToAdminsAsync();
        Task<bool> CheckContinousProjectionIsCreatedAsync(string name, string query);
        Task<bool> CheckOpsUserIsInAdminGroupAsync();
        Task<bool> CheckProjectionIsRunningAsync(string name);
        Task CreateContinousProjectionAsync(string name, string query, bool trackEmittedStreams = false, bool? emitEnabled = false, bool resetIfUpdated = false);
        Task CreatePersistentSubscriptionAsync(string stream, string group);
        Task DisableProjectionAsync(string name);
        Task EnableProjectionAsync(string name);
        Task ResetProjectionAsync(string name);
    }
}