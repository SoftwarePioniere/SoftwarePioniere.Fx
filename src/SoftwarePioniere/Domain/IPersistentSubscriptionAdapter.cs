﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.Domain
{
    public interface IPersistentSubscriptionAdapter<out T>
    {
        Task ConnectToPersistentSubscription(string stream, string groupName, ILogger logger, Func<T, Task> eventAppeared, int bufferSize = 10, bool skipRemoved = true);
    }
}