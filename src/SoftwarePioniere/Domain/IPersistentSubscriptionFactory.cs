namespace SoftwarePioniere.Domain
{
    public interface IPersistentSubscriptionFactory
    {
        IPersistentSubscriptionAdapter<T> CreateAdapter<T>();
    }
}