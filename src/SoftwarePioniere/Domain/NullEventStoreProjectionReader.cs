using System;
using System.Threading.Tasks;

namespace SoftwarePioniere.Domain
{
    public class NullEventStoreReader : IEventStoreReader
    {
        public Task<T> GetProjectionStateAsync<T>(string name, string partitionId = null)
        {
            var ret = default(T);
            return Task.FromResult(ret);
        }

        public Task<T> GetProjectionStateAsyncAnonymousType<T>(string name, T anonymousTypeObject, string partitionId = null)
        {
            return Task.FromResult(anonymousTypeObject);
        }

        public Task<string> RunQueryAsync(string name, string query)
        {
            return Task.FromResult(string.Empty);
        }

        public Task ReadStreamAndDoAsync(string streamName, Action<MyRecordedEvent> action, int count = 200)
        {
            return Task.CompletedTask;
        }

        public Task ReadStreamAndDoAsync(string streamName, Func<MyRecordedEvent, Task> task, int count = 200)
        {
            return Task.CompletedTask;
        }
    }
}
