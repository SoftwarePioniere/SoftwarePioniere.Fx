using System.Threading.Tasks;

namespace SoftwarePioniere.Messaging.Notifications
{
    public interface INotificationClient
    {
        Task Message(string message);

        Task Notitfy<T>(T notification) where T : class, INotificationMessage;
    }
}
