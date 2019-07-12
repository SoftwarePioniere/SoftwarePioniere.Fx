namespace SoftwarePioniere.Messaging.Notifications
{
    public interface INotificationContent
    {
        /// <summary>
        /// Message Type Name
        /// </summary>        
        string NotificationType { get; }
    }
}