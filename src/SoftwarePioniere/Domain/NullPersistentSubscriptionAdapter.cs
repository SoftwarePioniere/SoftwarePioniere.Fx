using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.Domain
{
    public class NullPersistentSubscriptionAdapter<T> : IPersistentSubscriptionAdapter<T>
    {
        public Task ConnectToPersistentSubscription(string stream, string groupName, ILogger logger, Func<T, IDictionary<string, string>, Task> eventAppeared,
            int bufferSize = 10, bool skipRemoved = true)
        {
            return Task.CompletedTask;
        }
    }
}