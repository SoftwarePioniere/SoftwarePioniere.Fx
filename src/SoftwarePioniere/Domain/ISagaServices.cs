using Foundatio.Lock;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Domain
{
    public interface ISagaServices
    {
        IMessageBusAdapter Bus { get; }
        IRepository Repository { get; }
        ILockProvider LockProvider { get; }
        IPersistentSubscriptionFactory PersistentSubscriptionFactory { get; }
    }
}