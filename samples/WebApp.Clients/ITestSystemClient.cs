using System.Threading.Tasks;

namespace WebApp.Clients
{
    public interface ITestSystemClient
    {
        Task<ModelA> GetForbidden();

        Task<ModelA> GetOk();

        Task<ModelA> GetNoContent();

        Task<ModelA> GetBadRequest();
    }
}
