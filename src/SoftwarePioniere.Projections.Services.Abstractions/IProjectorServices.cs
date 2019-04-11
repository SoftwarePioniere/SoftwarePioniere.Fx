using Foundatio.Lock;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Projections
{
    public interface IProjectorServices
    {
        ITelemetryAdapter TelemetryAdapter { get; }
        ICacheAdapter Cache { get; }
        IMessageBusAdapter Bus { get; }
        ILockProvider LockProvider { get; }
    }
}