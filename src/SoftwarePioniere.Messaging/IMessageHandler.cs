using System.Threading;

namespace SoftwarePioniere.Messaging
{
    /// <summary>
    /// Interface for a Message Handler
    /// </summary>
    public interface IMessageHandler
    {
        void Initialize(CancellationToken cancellationToken = default);
    }
}
