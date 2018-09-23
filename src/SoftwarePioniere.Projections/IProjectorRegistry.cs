using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Projections
{

    public class ProjectionInfo
    {
        public string ProjectorId { get; set; }
        public string StreamName { get; set; }
        public string Status { get; set; }
    }

    public interface IProjectorRegistry
    {
        Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken));

        ProjectionInfo[] Infos { get; }
    }
}
