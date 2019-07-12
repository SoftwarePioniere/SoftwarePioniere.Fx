namespace SoftwarePioniere.Messaging.Notifications
{
    /// <summary>
    /// Message to send over websocket
    /// </summary>
    public interface INotificationMessage : IMessage
    {
        /// <summary>
        /// Serialized Content
        /// </summary>
        string Content { get; }

        /// <summary>
        /// Message Type Name
        /// </summary>        
        string NotificationType { get; }

        /// <summary>
        /// Name of the User
        /// </summary>
        string UserName { get; }

    }
}
