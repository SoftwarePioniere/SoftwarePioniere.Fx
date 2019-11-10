using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.Domain
{

    public interface IRepository
    {

        Task SaveAsync<T>(T aggregate, CancellationToken token = default) where T : AggregateRoot;
        Task SaveAsync<T>(T aggregate, int expectedVersion, CancellationToken token = default) where T : AggregateRoot;
        Task<bool> CheckAggregateExists<T>(string aggregateId, CancellationToken token = default) where T : AggregateRoot;
        Task<T> GetByIdAsync<T>(string id, CancellationToken token = default) where T : AggregateRoot, new();
        Task<T> GetByIdAsync<T>(string id, int expectedAggregateVersion, CancellationToken token = default) where T : AggregateRoot, new();
        Task<T> GetByIdAsync<T>(string id, string streamName, CancellationToken token = default) where T : AggregateRoot, new();
        Task<T> GetByIdAsync<T>(string id, int expectedAggregateVersion, string streamName, CancellationToken token = default) where T : AggregateRoot, new();
    }
    
}