using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SoftwarePioniere.Domain
{
    /// <summary>
    /// Reads the State of the EventStore Projection
    /// </summary>
    public interface IEventStoreReader
    {
        Task<T> GetProjectionStateAsync<T>(string name, string partitionId = null);

        Task<T> GetProjectionStateAsyncAnonymousType<T>(string name, T anonymousTypeObject, string partitionId = null);

        Task<string> RunQueryAsync(string name, string query);

        Task ReadStreamAndDoAsync(string streamName, Action<MyRecordedEvent> action, int count = 200);

        Task ReadStreamAndDoAsync(string streamName, Func<MyRecordedEvent, Task> task, int count = 200);
    }


    public class MyRecordedEvent
    {
        public MyRecordedEvent(long eventNumber, string eventType, DateTime created, JObject data, JObject meta)
        {
            EventNumber = eventNumber;
            EventType = eventType;
            Created = created;
            Data = data;
            Meta = meta;
        }

        public long EventNumber { get; }
        public string EventType { get; }
        public DateTime Created { get; }
        public JObject Data { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public JObject Meta { get; }
    }
}
