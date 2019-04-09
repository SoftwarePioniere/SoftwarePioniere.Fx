using Foundatio.Lock;
using SoftwarePioniere.DomainModel.Subscriptions;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel
{
    public interface ISagaServices
    {
        IMessageBusAdapter Bus { get; }
        IRepository Repository { get; }
        ILockProvider LockProvider { get; }
        IPersistentSubscriptionFactory PersistentSubscriptionFactory { get; }
    }
}