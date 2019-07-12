using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Domain
{
    public interface IEventStoreInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken));

        int ExecutionOrder { get; }
    }
}
