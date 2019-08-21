using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SoftwarePioniere;

namespace WebApp
{

    // ReSharper disable once ClassNeverInstantiated.Global
    [Authorize]
    public class TestHubAuth : Hub
    {
        private readonly ILogger _logger;

        public TestHubAuth(ILoggerFactory loggerFactory)
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

            var userId = Context.User?.GetNameIdentifier();
            _logger.LogInformation("User {UserId} connected with Id {ConnectionId}", userId, Context.ConnectionId);
            await Clients.All.SendAsync("message", $"User: {userId} connected with Id {Context.ConnectionId}");

            await base.OnConnectedAsync();
        }

        /// <inheritdoc />
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            _logger.LogDebug("OnDisconnectedAsync");
            var userId = Context.User?.GetNameIdentifier();

            if (ex != null)
            {
                _logger.LogError(ex, "User {UserId} disconnected with Id {ConnectionId} and Error ", userId, Context.ConnectionId);
            }
            
            _logger.LogInformation("User {UserId} disconnected with Id {ConnectionId}", userId, Context.ConnectionId);
            await Clients.All.SendAsync("message", $"User: {userId} disconnected with Id {Context.ConnectionId}");

            await base.OnDisconnectedAsync(ex);
        }
    }
}
