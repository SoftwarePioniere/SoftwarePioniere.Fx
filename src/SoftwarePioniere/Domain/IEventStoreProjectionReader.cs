using System.Threading.Tasks;

namespace SoftwarePioniere.Domain
{
    /// <summary>
    /// Reads the State of the EventStore Projection
    /// </summary>
    public interface IEventStoreProjectionReader
    {
        Task<T> GetStateAsync<T>(string name, string partitionId = null);

        Task<T> GetStateAsyncAnonymousType<T>(string name, T anonymousTypeObject, string partitionId = null);
    }
}
