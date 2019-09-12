using System;
using Foundatio.Lock;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Telemetry;

namespace SoftwarePioniere.Projections
{
    public class ProjectorServices : IProjectorServices
    {
        public ProjectorServices(ITelemetryAdapter telemetryAdapter, ICacheAdapter cache, IMessageBusAdapter bus, ILockProvider lockProvider, IServiceProvider serviceProvider)
        {
            TelemetryAdapter = telemetryAdapter ?? throw new ArgumentNullException(nameof(telemetryAdapter));
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
            LockProvider = lockProvider ?? throw new ArgumentNullException(nameof(lockProvider));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        public ITelemetryAdapter TelemetryAdapter { get; }
        public ICacheAdapter Cache { get; }
        public IMessageBusAdapter Bus { get; }
        public ILockProvider LockProvider { get; }
        public IServiceProvider ServiceProvider { get; }
    }
}