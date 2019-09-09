using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Hosting
{
    public interface IConnectionProvider
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
