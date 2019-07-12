using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Domain
{
    public interface ISaga
    {
        Task StartAsync(CancellationToken cancellationToken);
    }
}
