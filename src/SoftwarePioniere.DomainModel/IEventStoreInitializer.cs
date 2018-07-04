using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.DomainModel
{
    public interface IEventStoreInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}
