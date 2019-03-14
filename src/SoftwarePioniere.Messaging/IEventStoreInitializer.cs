using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Messaging
{
    public interface IEventStoreInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken));

        int ExecutionOrder { get; }
    }
}
