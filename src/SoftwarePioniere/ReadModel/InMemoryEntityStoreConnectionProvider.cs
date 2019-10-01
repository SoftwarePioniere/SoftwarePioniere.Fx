using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.ReadModel
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InMemoryEntityStoreConnectionProvider : IEntityStoreConnectionProvider
    {
        public ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> Items { get; private set; }

        public InMemoryEntityStoreConnectionProvider()
        {
            Items = new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();
        }

        public Task ClearDatabaseAsync()
        {
            Items = new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();
            return Task.CompletedTask;
        }

        public ConcurrentDictionary<string, object> GetItems(Type t)
        {
            return Items.GetOrAdd(t, new ConcurrentDictionary<string, object>());
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
