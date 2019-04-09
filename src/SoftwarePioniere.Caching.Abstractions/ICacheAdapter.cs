using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Caching
{
    public interface ICacheAdapter
    {
        Task<T> CacheLoad<T>(Func<Task<T>> loader, string cacheKey,
            int minutes = 60, ILogger logger = null);

        Task<T> CacheLoadItem<T>(Func<Task<T>> loader, string cacheKey,
            int minutes = 60, ILogger logger = null);

        Task<T[]> CacheLoadItems<T>(Func<Task<T[]>> loader, string cacheKey,
            int minutes = 60, ILogger logger = null);

        Task<PagedResults<T>> CacheLoadPagedItems<T>(Func<Task<PagedResults<T>>> loader, string cacheKey,
            int minutes = 60, ILogger logger = null);

        Task<int> RemoveByPrefixAsync(string prefix);

        Task<bool> AddAsync<T>(string key, T value);

        //Task LockPrefix(string prefix);

        //Task ReleasePrefix(string prefix);
        
    }
    
    public interface ICacheKeyBuilder
    {
        ICacheKeyBuilder WithPrefix(string prefix);

        //ICacheKeyBuilder Append(IEnumerable<string> values);

        ICacheKeyBuilder Append(string v1);

        //ICacheKeyBuilder Append(string v1, int i1, int i2);

        //ICacheKeyBuilder ForReadModel<T>()
    }
}
