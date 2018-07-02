namespace SoftwarePioniere.DomainModel
{
    /// <summary>
    /// Ein Event Anwenden, wird im Aggregate verwendet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IApplyEvent<in T>
    {
        /// <summary>
        /// Wendet die Event Daten an und verändert so den Zustand des Aggregats
        /// </summary>
        /// <param name="message"></param>
        // ReSharper disable once UnusedMember.Global
        void ApplyEvent(T message);
    }
}
