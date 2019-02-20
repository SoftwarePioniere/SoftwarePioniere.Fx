using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.DomainModel
{
    public interface ISaga
    {
        Task StartAsync(CancellationToken cancellationToken);
    }
}
