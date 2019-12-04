using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.ReadModel
{
    public interface IEntityStoreInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken));

        int ExecutionOrder { get; }
    }
}
