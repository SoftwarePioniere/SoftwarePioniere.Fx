namespace SoftwarePioniere.DomainModel.Subscriptions
{
    public interface IPersistentSubscriptionFactory
    {
        IPersistentSubscriptionAdapter<T> CreateAdapter<T>();
    }

    public class NullPersistentSubscriptionFactory : IPersistentSubscriptionFactory
    {
        public IPersistentSubscriptionAdapter<T> CreateAdapter<T>()
        {
            return null;
        }
    }
}