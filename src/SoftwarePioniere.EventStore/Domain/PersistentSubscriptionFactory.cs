using System;
using Microsoft.Extensions.Hosting;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Telemetry;

namespace SoftwarePioniere.EventStore.Domain
{
    public class PersistentSubscriptionFactory : IPersistentSubscriptionFactory
    {
        private readonly EventStoreConnectionProvider _connectionProvider;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ITelemetryAdapter _telemetryAdapter;

        public PersistentSubscriptionFactory(EventStoreConnectionProvider connectionProvider, IApplicationLifetime applicationLifetime, ITelemetryAdapter telemetryAdapter)
        {
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            _telemetryAdapter = telemetryAdapter ?? throw new ArgumentNullException(nameof(telemetryAdapter));
        }


        public IPersistentSubscriptionAdapter<T> CreateAdapter<T>()
        {
            return new PersistentSubscriptionAdapter<T>(_connectionProvider, _applicationLifetime, _telemetryAdapter);
        }
    }
}