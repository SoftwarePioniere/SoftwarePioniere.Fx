namespace SoftwarePioniere.DomainModel.Subscriptions
{
    public class NullPersistentSubscriptionFactory : IPersistentSubscriptionFactory
    {
        public IPersistentSubscriptionAdapter<T> CreateAdapter<T>()
        {
            return null;
        }
    }
}