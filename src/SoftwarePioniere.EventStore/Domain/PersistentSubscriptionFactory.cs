using System;
using Microsoft.Extensions.Hosting;
using SoftwarePioniere.Domain;

namespace SoftwarePioniere.EventStore.Domain
{
    public class PersistentSubscriptionFactory : IPersistentSubscriptionFactory
    {
        private readonly EventStoreConnectionProvider _connectionProvider;
        private readonly IApplicationLifetime _applicationLifetime;
  
        public PersistentSubscriptionFactory(EventStoreConnectionProvider connectionProvider, IApplicationLifetime applicationLifetime)
        {
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }


        public IPersistentSubscriptionAdapter<T> CreateAdapter<T>()
        {
            return new PersistentSubscriptionAdapter<T>(_connectionProvider, _applicationLifetime);
        }
    }
}