using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.ReadModel
{
    public interface IEntityStore
    {
        Task DeleteItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity;

        Task DeleteItemsAsync<T>(Expression<Func<T, bool>> where, CancellationToken token = default(CancellationToken)) where T : Entity;

        Task DeleteAllItemsAsync<T>(CancellationToken token = default(CancellationToken)) where T : Entity;

        Task InsertItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;

        Task BulkInsertItemsAsync<T>(IEnumerable<T> items, CancellationToken token = default(CancellationToken)) where T : Entity;

        Task InsertOrUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;

        Task<T> LoadItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity;

        Task<T[]> LoadItemsAsync<T>(CancellationToken token = default(CancellationToken)) where T : Entity;

        Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, CancellationToken token = default(CancellationToken)) where T : Entity;

        //Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms, CancellationToken token = default(CancellationToken)) where T : Entity;

        Task UpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;
    }
}