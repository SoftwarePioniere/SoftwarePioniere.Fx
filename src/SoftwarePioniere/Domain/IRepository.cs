using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Domain
{

    public interface IRepository
    {

        Task<IEnumerable<AggregateDomainEventMessage>> SaveAsyncWithOutPush<T>(T aggregate, CancellationToken token = default) where T : AggregateRoot;
        Task<IEnumerable<AggregateDomainEventMessage>> SaveAsyncWithOutPush<T>(T aggregate, int expectedVersion, CancellationToken token = default) where T : AggregateRoot;

        Task SaveAsync<T>(T aggregate, CancellationToken token = default) where T : AggregateRoot;
        Task SaveAsync<T>(T aggregate, int expectedVersion, CancellationToken token = default) where T : AggregateRoot;
        Task<bool> CheckAggregateExists<T>(string aggregateId, CancellationToken token = default) where T : AggregateRoot;
        Task<T> GetByIdAsync<T>(string aggregateId, CancellationToken token = default) where T : AggregateRoot, new();
        Task<T> GetByIdAsync<T>(string aggregateId, int expectedVersion, CancellationToken token = default) where T : AggregateRoot, new();
        Task<T> GetByIdAsync<T>(string aggregateId, string streamName, CancellationToken token = default) where T : AggregateRoot, new();
        Task<T> GetByIdAsync<T>(string aggregateId, int expectedVersion, string streamName, CancellationToken token = default) where T : AggregateRoot, new();
    }
    
}