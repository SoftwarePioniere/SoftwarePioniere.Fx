using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Projections
{
    public interface IProjector
    {
        Task StartSubscriptionAsync(CancellationToken token = default);

        Task RunAsync(CancellationToken token = default);
    }
}
