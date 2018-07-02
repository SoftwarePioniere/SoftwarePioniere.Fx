namespace SoftwarePioniere.Messaging
{
    public interface ICommand : IMessage
    {
        /// <summary>
        /// Die EventVersion des Original Objekts
        /// </summary>     
        int OriginalVersion { get; }

        /// <summary>
        /// Correlation Id des Requests, z.B. vom Webserver
        /// </summary>
        string RequestId { get; }

    }
}