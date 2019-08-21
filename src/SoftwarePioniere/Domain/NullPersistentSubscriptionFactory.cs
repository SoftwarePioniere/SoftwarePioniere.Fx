namespace SoftwarePioniere.Domain
{
    public class NullPersistentSubscriptionFactory : IPersistentSubscriptionFactory
    {
        public IPersistentSubscriptionAdapter<T> CreateAdapter<T>()
        {
            return null;
        }
    }
}