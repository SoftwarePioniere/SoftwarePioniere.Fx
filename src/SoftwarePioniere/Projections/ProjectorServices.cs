using System;
using Foundatio.Lock;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Projections
{
    public class ProjectorServices : IProjectorServices
    {
        public ProjectorServices(ICacheAdapter cache, IMessageBusAdapter bus, ILockProvider lockProvider, IServiceProvider serviceProvider)
        {
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
            LockProvider = lockProvider ?? throw new ArgumentNullException(nameof(lockProvider));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        public ICacheAdapter Cache { get; }
        public IMessageBusAdapter Bus { get; }
        public ILockProvider LockProvider { get; }
        public IServiceProvider ServiceProvider { get; }
    }
}