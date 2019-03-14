using System.Threading.Tasks;

namespace SoftwarePioniere.Messaging
{
    public class EmptyEventStoreSetup : IEventStoreSetup
    {
        public Task AddOpsUserToAdminsAsync()
        {
            return Task.CompletedTask;
        }

        public Task<bool> CheckContinousProjectionIsCreatedAsync(string name, string query)
        {
            return Task.FromResult(true);
        }

        public Task<bool> CheckOpsUserIsInAdminGroupAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> CheckProjectionIsRunningAsync(string name)
        {
            return Task.FromResult(true);
        }

        public Task CreateContinousProjectionAsync(string name, string query, bool trackEmittedStreams = false, bool? emitEnabled)
        {
            return Task.CompletedTask;
        }

        public Task CreatePersistentSubscriptionAsync(string stream, string @group)
        {
            return Task.CompletedTask;
        }

        public Task DisableProjectionAsync(string name)
        {
            return Task.CompletedTask;
        }

        public Task EnableProjectionAsync(string name)
        {
            return Task.CompletedTask;
        }
    }
}