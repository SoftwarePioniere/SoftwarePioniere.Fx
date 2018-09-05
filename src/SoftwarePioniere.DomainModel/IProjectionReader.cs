using System.Threading.Tasks;

namespace SoftwarePioniere.DomainModel
{
    /// <summary>
    /// Reads the State of the EventStore Projection
    /// </summary>
    public interface IProjectionReader
    {
        Task<T> GetStateAsync<T>(string name, string partitionId = null);
    }
}
