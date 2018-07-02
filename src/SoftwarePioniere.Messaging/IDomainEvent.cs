namespace SoftwarePioniere.Messaging
{
    public interface IDomainEvent : IMessage
    {
        //int EventVersion { get; }

        string AggregateId { get; }
    }
}