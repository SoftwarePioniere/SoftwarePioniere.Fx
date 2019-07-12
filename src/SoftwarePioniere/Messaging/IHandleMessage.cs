using System.Threading.Tasks;

namespace SoftwarePioniere.Messaging
{
    /// <summary>
    /// Auf eine beliebige Nachricht reagieren
    /// Muss im internen Bus registriert sein    
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHandleMessage<in T> where T : IMessage
    {
        /// <summary>
        /// Message verarbeiten
        /// </summary>
        /// <param name="message"></param>
        Task HandleAsync(T message);
     
    }
}