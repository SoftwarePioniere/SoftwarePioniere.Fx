using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftwarePioniere.ReadModel.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InMemoryEntityStoreConnectionProvider : IEntityStoreConnectionProvider
    {
        public Dictionary<Type, Dictionary<string, object>> Items { get; private set; }

        private readonly object _lock = new object();

        public InMemoryEntityStoreConnectionProvider()
        {
            Items = new Dictionary<Type, Dictionary<string, object>>();
        }

        public Task ClearDatabaseAsync()
        {
            Items = new Dictionary<Type, Dictionary<string, object>>();
            return Task.CompletedTask;
        }

        public Dictionary<string, object> GetItems(Type t)
        {
            lock (_lock)
            {
                if (!Items.ContainsKey(t))
                {
                    Items.Add(t, new Dictionary<string, object>());
                }

                return Items[t];
            }
        }
    }
}
