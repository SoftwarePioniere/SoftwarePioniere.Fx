using System.Threading.Tasks;

namespace SoftwarePioniere.ReadModel
{
    public interface IEntityStoreConnectionProvider
    {
        /// <summary>
        /// leert die gesamte datenbank
        /// </summary>
        /// <returns></returns>
        Task ClearDatabaseAsync();
    }
}