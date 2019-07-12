namespace SoftwarePioniere.Domain
{
    /// <summary>
    /// Interface für eine Aggregat Id
    /// </summary>
    public interface IAggregateId
    {
        /// <summary>
        /// Gibt die erzeugte Id zurück
        /// </summary>
        string Id { get; }
    }

    public abstract class BaseAggregateId : IAggregateId
    { 
        public string Id { get; }

        protected BaseAggregateId(string id)
        {
            Id = id;
        }
    }
}
