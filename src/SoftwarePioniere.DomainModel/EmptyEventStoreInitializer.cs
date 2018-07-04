using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.DomainModel
{
    public class EmptyEventStoreInitializer : IEventStoreInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
