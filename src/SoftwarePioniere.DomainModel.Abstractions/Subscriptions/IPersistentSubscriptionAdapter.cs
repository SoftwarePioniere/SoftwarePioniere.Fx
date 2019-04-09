using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.DomainModel.Subscriptions
{
    public interface IPersistentSubscriptionAdapter<out T>
    {
        Task ConnectToPersistentSubscription(string stream,
            string groupName, ILogger logger
            , Func<T, IDictionary<string, string>, Task> eventAppeared,
            int bufferSize = 10, bool skipRemoved = true);
    }
}