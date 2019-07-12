using System.Security.Claims;
using System.Threading.Tasks;

namespace SoftwarePioniere.Sample.WebApp.Clients
{
    public interface ITestUserClient
    {
        Task<ModelA> GetForbidden(ClaimsPrincipal user);

        Task<ModelA> GetOk(ClaimsPrincipal user);

        Task<ModelA> GetNoContent(ClaimsPrincipal user);

        Task<ModelA> GetBadRequest(ClaimsPrincipal user);
    }
}
