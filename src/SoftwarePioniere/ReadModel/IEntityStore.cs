using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.ReadModel
{
    /// <summary>
    ///     Dienst zum Laden und Speichern von Objekten
    /// </summary>
    public interface IEntityStore
    {
        /// <summary>
        ///     Ein Objekt löschen
        /// </summary>     
        Task DeleteItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity;

        /// <summary>
        ///     Ein Objekt mit dem Element Schlüssel speichern
        /// </summary>      
        Task InsertItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;

        Task BulkInsertItemsAsync<T>(IEnumerable<T> items, CancellationToken token = default(CancellationToken)) where T : Entity;

        /// <summary>
        ///     Fügt ein Entity ein, falls es nicht existiert, sonst update
        /// </summary>    
        /// <returns></returns>
        Task InsertOrUpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;

        /// <summary>
        ///     Das Objekt mit dem angegeben Elemet Schlüssel Laden
        /// </summary>       
        /// <returns></returns>
        Task<T> LoadItemAsync<T>(string entityId, CancellationToken token = default(CancellationToken)) where T : Entity;

        /// <summary>
        ///     Alle Elemente zurück geben
        /// </summary>
        /// <returns></returns>
        Task<T[]> LoadItemsAsync<T>(CancellationToken token = default(CancellationToken)) where T : Entity;

        /// <summary>
        ///     Alle Elemente mit where gefiltert zurück geben
        /// </summary>
        /// <returns></returns>
        Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, CancellationToken token = default(CancellationToken)) where T : Entity;

        ///// <summary>
        ///// Alle Elemente mit where gefieltert zurück geben, es wird aber vorher im cahe geprüft, ob es den eintrage bereits gibt
        ///// </summary>        
        ///// <returns></returns>
        //Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, string cacheKey, CancellationToken token = default(CancellationToken)) where T : Entity;

        /// <summary>
        ///     Eine Paged List laden
        /// </summary>
        /// <returns></returns>
        Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms, CancellationToken token = default(CancellationToken)) where T : Entity;

        ///// <summary>
        /////     Eine Paged List laden, vorher mit im Cache geprüft, ob es den eintrag bereits gibts
        ///// </summary>
        ///// <returns></returns>
        //Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms, string cacheKey, CancellationToken token = default(CancellationToken)) where T : Entity;

        /// <summary>
        ///     Aktualisieren eines elements
        /// </summary>
        /// <returns></returns>
        Task UpdateItemAsync<T>(T item, CancellationToken token = default(CancellationToken)) where T : Entity;
    }
}