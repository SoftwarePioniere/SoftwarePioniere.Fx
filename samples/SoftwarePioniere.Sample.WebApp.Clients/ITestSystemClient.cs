using System.Threading.Tasks;

namespace SoftwarePioniere.Sample.WebApp.Clients
{
    public interface ITestSystemClient
    {
        Task<ModelA> GetForbidden();

        Task<ModelA> GetOk();

        Task<ModelA> GetNoContent();

        Task<ModelA> GetBadRequest();
    }
}
