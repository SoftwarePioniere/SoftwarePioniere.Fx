using System;
using Foundatio.Lock;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Telemetry;

namespace SoftwarePioniere.Projections
{
    public interface IProjectorServices
    {
        ITelemetryAdapter TelemetryAdapter { get; }
        ICacheAdapter Cache { get; }
        IMessageBusAdapter Bus { get; }
        ILockProvider LockProvider { get; }
        IServiceProvider ServiceProvider { get; }
    }
}