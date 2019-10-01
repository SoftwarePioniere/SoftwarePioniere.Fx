using System.Threading.Tasks;
using SoftwarePioniere.Hosting;

namespace SoftwarePioniere.ReadModel
{
    public interface IEntityStoreConnectionProvider : IConnectionProvider
    {
        /// <summary>
        /// leert die gesamte datenbank
        /// </summary>
        /// <returns></returns>
        Task ClearDatabaseAsync();
    }
}