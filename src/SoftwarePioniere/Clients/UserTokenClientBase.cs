using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SoftwarePioniere.Clients
{

    /// <summary>
    /// Client, der das Token aus dem User zieht
    /// </summary>
    public abstract class UserTokenClientBase
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly HttpClient Client;

        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly ILogger Logger;

        protected UserTokenClientBase(ILoggerFactory loggerFactory, HttpClient client)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            Logger = loggerFactory.CreateLogger(GetType());
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public static string GetNameIdentifier(ClaimsPrincipal user)
        {
            const string claim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
            var value = user.Claims.FirstOrDefault(c => c.Type == claim)?.Value;

            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException("no id claim in token: " + claim);

            return value;
        }


        protected virtual async Task<T> GetAsAsync<T>(ClaimsPrincipal user, string uri)
        {

            var response = await GetAsync(user, uri);

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new AuthenticationException(response.ReasonPhrase);
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default(T);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException(response.ReasonPhrase);
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        protected virtual async Task<HttpResponseMessage> GetAsync(ClaimsPrincipal user, string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            if (!request.Headers.Contains("Authorization"))
            {

                var accessToken = user.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                if (string.IsNullOrEmpty(accessToken)) throw new InvalidOperationException("no access_token claim in user principal");

                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                //request.Headers.Add("Content-Type", "application/json");
                //request.Headers.Add("Accept", "application/json");
            }


            var response = await Client.SendAsync(request);
            //response.EnsureSuccessStatusCode();

            return response;
        }
    }
}
