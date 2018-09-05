using System.Threading.Tasks;

namespace SoftwarePioniere.DomainModel.Services
{
    public class NullProjectionReader : IProjectionReader
    {
        public Task<T> GetStateAsync<T>(string name, string partitionId = null)
        {
            var ret = default(T);
            return Task.FromResult(ret);
        }
    }
}
