namespace SoftwarePioniere.Messaging
{
    public abstract class EventsTestBase<T> : MessageTestBase<T> where T : class, IDomainEvent
    {      
        protected const string AggregateId = "fake aggregate id";
        
    }
}