namespace SoftwarePioniere.DomainModel.Subscriptions
{
    public interface IPersistentSubscriptionFactory
    {
        IPersistentSubscriptionAdapter<T> CreateAdapter<T>();
    }
}