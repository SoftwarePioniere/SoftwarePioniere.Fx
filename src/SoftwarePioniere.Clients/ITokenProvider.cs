using System.Threading.Tasks;

namespace SoftwarePioniere.Clients
{
   
    public interface ITokenProvider
    {    
        Task<string> GetAccessToken(string resource);
    }
}