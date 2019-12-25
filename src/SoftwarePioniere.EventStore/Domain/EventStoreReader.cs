using System;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoftwarePioniere.Domain;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SoftwarePioniere.EventStore.Domain
{
    public class EventStoreReader : IEventStoreReader
    {
        private readonly ILogger _logger;
        private readonly EventStoreConnectionProvider _provider;

        public EventStoreReader(EventStoreConnectionProvider provider, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(GetType());
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        }

        public async Task<T> GetProjectionStateAsync<T>(string name, string partitionId = null)
        {
            _logger.LogDebug("GetStateAsync {Type} {ProjectionName} {PartitionId}", typeof(T).Name, name, partitionId);

            var json = await LoadJsonAsync(name, partitionId);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<T> GetProjectionStateAsyncAnonymousType<T>(string name, T anonymousTypeObject, string partitionId = null)
        {
            _logger.LogDebug("GetStateAsyncAnonymousType {Type} {ProjectionName} {PartitionId}", typeof(T).Name, name, partitionId);

            var json = await LoadJsonAsync(name, partitionId);
            return JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
        }

        public Task<string> RunQueryAsync(string name, string query)
        {
            _logger.LogDebug("RunQueryAsync {Query}", query);

            var options = _provider.Options;

            var queryMan = _provider.CreateQueryManager();
            return queryMan.ExecuteAsync(name, query,
                TimeSpan.FromSeconds(options.InitialPollingDelaySeconds),
                TimeSpan.FromSeconds(options.MaximumPollingDelaySeconds), _provider.OpsCredentials);
        }

        public async Task ReadStreamAndDoAsync(string streamName, Action<MyRecordedEvent> action, int count = 200)
        {
            _logger.LogDebug("ReadStreamAndDoAsync {StreamName}", streamName);

            long sliceStart = StreamPosition.Start;
            StreamEventsSlice currentSlice;

            var con = await _provider.GetActiveConnection();
            do
            {
                currentSlice =
                    await con.ReadStreamEventsForwardAsync(streamName,
                        sliceStart,
                        count,
                        true,
                        _provider.OpsCredentials);

                if (currentSlice.Status == SliceReadStatus.Success)
                {

                    sliceStart = currentSlice.NextEventNumber;

                    foreach (var evnt in currentSlice.Events)
                    {
                        var ev = evnt.Event;
                        if (ev.IsJson)
                        {
                            var dataJson = Encoding.UTF8.GetString(ev.Data);
                            var dataJo = JObject.Parse(dataJson);

                            var metaJson = Encoding.UTF8.GetString(ev.Metadata);
                            var metaJo = JObject.Parse(metaJson);

                            action(new MyRecordedEvent(ev.EventNumber, ev.EventType, ev.Created, dataJo, metaJo));
                        }
                    }
                }

            } while (!currentSlice.IsEndOfStream);
        }

        public async Task ReadStreamAndDoAsync(string streamName, Func<MyRecordedEvent, Task> task, int count = 200)
        {
            _logger.LogDebug("ReadStreamAndDoAsync {StreamName}", streamName);

            long sliceStart = StreamPosition.Start;
            StreamEventsSlice currentSlice;

            var con = await _provider.GetActiveConnection();
            do
            {
                currentSlice =
                    await con.ReadStreamEventsForwardAsync(streamName,
                        sliceStart,
                        count,
                        true,
                        _provider.OpsCredentials);

                sliceStart = currentSlice.NextEventNumber;

                foreach (var evnt in currentSlice.Events)
                {
                    var ev = evnt.Event;
                    if (ev.IsJson)
                    {
                        var dataJson = Encoding.UTF8.GetString(ev.Data);
                        var dataJo = JObject.Parse(dataJson);

                        var metaJson = Encoding.UTF8.GetString(ev.Metadata);
                        var metaJo = JObject.Parse(metaJson);

                        await task(new MyRecordedEvent(ev.EventNumber, ev.EventType, ev.Created, dataJo, metaJo));
                    }
                }
            } while (!currentSlice.IsEndOfStream);
        }

        private async Task<string> LoadJsonAsync(string name, string partitionId)
        {
            var manager = _provider.CreateProjectionsManager();

            string json;

            if (string.IsNullOrEmpty(partitionId))
            {
                json = await manager.GetStateAsync(name, _provider.OpsCredentials);
            }
            else
            {
                json = await manager.GetPartitionStateAsync(name, partitionId, _provider.OpsCredentials);
            }

            _logger.LogTrace("State {StateJson}", json);
            return json;
        }
    }
}
