using System;
using System.Linq.Expressions;
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
        /// <param name="entityId">The key the object is stored using</param>
        Task DeleteItemAsync<T>(string entityId) where T : Entity;

        /// <summary>
        ///     Ein Objekt mit dem Element Schlüssel speichern
        /// </summary>
        /// <param name="item">The object to store</param>
        Task InsertItemAsync<T>(T item) where T : Entity;

        /// <summary>
        ///     Fügt ein Entity ein, falls es nicht existiert, sonst update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        Task InsertOrUpdateItemAsync<T>(T item) where T : Entity;

        /// <summary>
        ///     Das Objekt mit dem angegeben Elemet Schlüssel Laden
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        Task<T> LoadItemAsync<T>(string entityId) where T : Entity;

        /// <summary>
        ///     Alle Elemente zurück geben
        /// </summary>
        /// <returns></returns>
        Task<T[]> LoadItemsAsync<T>() where T : Entity;

        /// <summary>
        ///     Alle Elemente mit where gefiltert zurück geben
        /// </summary>
        /// <returns></returns>
        Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where) where T : Entity;

        /// <summary>
        /// Alle Elemente mit where gefieltert zurück geben, es wird aber vorher im cahe geprüft, ob es den eintrage bereits gibt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<T[]> LoadItemsAsync<T>(Expression<Func<T, bool>> where, string cacheKey) where T : Entity;

        /// <summary>
        ///     Eine Paged List laden
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms) where T : Entity;

        /// <summary>
        ///     Eine Paged List laden, vorher mit im Cache geprüft, ob es den eintrag bereits gibts
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<PagedResults<T>> LoadPagedResultAsync<T>(PagedLoadingParameters<T> parms, string cacheKey) where T : Entity;


        /// <summary>
        ///     Aktualisieren eines elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        Task UpdateItemAsync<T>(T item) where T : Entity;
    }
}