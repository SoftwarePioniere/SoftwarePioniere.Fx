using System;
using Foundatio.Lock;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Projections
{
    public interface IProjectorServices
    {
        ICacheAdapter Cache { get; }
        IMessageBusAdapter Bus { get; }
        ILockProvider LockProvider { get; }
        IServiceProvider ServiceProvider { get; }
    }
}