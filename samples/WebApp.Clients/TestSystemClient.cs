using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Clients;

namespace WebApp.Clients
{
    public class TestSystemClient : ITestSystemClient
    {
        private readonly HttpClient _client;
        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger _logger;

        public TestSystemClient(ILoggerFactory loggerFactory, HttpClient client)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger(GetType());
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task<ModelA> GetForbidden()
        {
            const string uri = "testclient/forbidden";
            return _client.GetAsAsync<ModelA>(uri);
        }

        public Task<ModelA> GetOk()
        {
            const string uri = "testclient/ok";
            return _client.GetAsAsync<ModelA>(uri);
        }

        public Task<ModelA> GetNoContent()
        {
            const string uri = "testclient/nocontent";
            return _client.GetAsAsync<ModelA>(uri);
        }

        public Task<ModelA> GetBadRequest()
        {
            const string uri = "testclient/badrequest";
            return _client.GetAsAsync<ModelA>(uri);
        }


    }
}
