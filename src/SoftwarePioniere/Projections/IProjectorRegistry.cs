using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Projections
{
    public interface IProjectorRegistry
    {
        Task StartAsync(CancellationToken cancellationToken);

        Task<ProjectionRegistryStatus> GetStatusAsync();
    }
}
