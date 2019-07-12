using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Messaging
{
    /// <summary>
    /// Interface for a Message Handler
    /// </summary>
    public interface IMessageHandler
    {
        Task StartAsync(CancellationToken cancellationToken);
    }
}
