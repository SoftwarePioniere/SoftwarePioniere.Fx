using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Clients;

namespace SoftwarePioniere.Sample.WebApp.Clients
{
    public class TestUserClient : UserTokenClientBase, ITestUserClient
    {
        public TestUserClient(ILoggerFactory loggerFactory, HttpClient client) : base(loggerFactory, client)
        {
        }

        public async Task<ModelA> GetForbidden(ClaimsPrincipal user)
        {
            const string uri = "testclient/forbidden";
            return await GetAsAsync<ModelA>(user, uri);
        }

        public async Task<ModelA> GetOk(ClaimsPrincipal user)
        {
            const string uri = "testclient/ok";
            return await GetAsAsync<ModelA>(user, uri);
        }

        public async Task<ModelA> GetNoContent(ClaimsPrincipal user)
        {
            const string uri = "testclient/nocontent";
            return await GetAsAsync<ModelA>(user, uri);
        }

        public async Task<ModelA> GetBadRequest(ClaimsPrincipal user)
        {
            const string uri = "testclient/badrequest";
            return await GetAsAsync<ModelA>(user, uri);
        }
    }
}
