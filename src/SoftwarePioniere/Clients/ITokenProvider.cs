using System.Threading.Tasks;

namespace SoftwarePioniere.Clients
{
   
    public interface ITokenProvider
    {    
        Task<string> GetAccessToken(string resource, string tenantId = "");

        Task<string> GetAccessToken(string resource, bool force, string tenantId = "");
    }
}