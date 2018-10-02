using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Projections
{
    public interface IProjectorRegistry
    {
        Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<ProjectionRegistryStatus> GetStatusAsync();
    }
}
