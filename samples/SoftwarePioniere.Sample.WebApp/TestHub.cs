using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.Sample.WebApp
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestHub : Hub
    {
        private readonly ILogger _logger;

        public TestHub(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger(GetType());
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogDebug("OnConnectedAsync");
            await Clients.All.SendAsync("message", $"{Context.ConnectionId} connected");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            _logger.LogDebug("OnDisconnectedAsync");
            if (ex != null)
            {
                _logger.LogError(ex, "Disconnected with error");
            }

            await Clients.All.SendAsync("message", $"{Context.ConnectionId} disconnected");

            await base.OnDisconnectedAsync(ex);
        }
    }
}
