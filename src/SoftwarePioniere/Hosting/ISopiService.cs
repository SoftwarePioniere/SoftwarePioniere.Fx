using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Hosting
{
    public interface ISopiService
    {
        Task StartAsync(CancellationToken cancellationToken);
    }
}
