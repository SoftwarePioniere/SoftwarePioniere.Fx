using System;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.AspNetCore;
using SoftwarePioniere.Messaging;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftwarePioniere.Sample.WebApp
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "cmd1")]
    public class MyCommandController : ControllerBase
    {
        private readonly ILogger _logger;

        public MyCommandController(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger(GetType());
        }

        [HttpPost("op2")]
        [SwaggerOperation(OperationId = "PostOperation2")]
        [Authorize]
        public Task<ActionResult<MessageResponse>> PostOperation2Async(MyRequest1 request
            , [FromServices] ITelemetryAdapter telemetry
            , [FromServices] IMessageBusAdapter bus
            )
        {
            return this.RunWithTelemetryAsync("COMMAND Operation2",
                (state) =>
                {
                    var cmd = request.CreateFakeCommand(HttpContext.User.GetNameIdentifier());

                    return bus.PublishCommandAsync(cmd, HttpContext.RequestAborted, state);
                },  _logger);
        }

        [HttpPost("op1")]
        [SwaggerOperation(OperationId = "PostOperation1")]
        [Authorize]
        public Task<ActionResult<MessageResponse>> PostOperation1Async(MyRequest1 request
                , [FromServices] ITelemetryAdapter telemetry
                , [FromServices] IMessageBusAdapter bus)
        {

            return this.RunWithTelemetryAsync("COMMAND Operation1",
                async (state) =>
                {

                    await ThrowAuthAsync(HttpContext.User);
                    var cmd = request.CreateFakeCommand(HttpContext.User.GetNameIdentifier());

                    return await bus.PublishCommandAsync(cmd, HttpContext.RequestAborted, state);

                },  _logger);


        }

        private Task ThrowAuthAsync(ClaimsPrincipal httpContextUser)
        {
            throw new AuthenticationException($"nur mal so, {httpContextUser.GetNameIdentifier()}");
        }

        [HttpPost("op1/multi")]
        [SwaggerOperation(OperationId = "PostOperation1Multi")]
        [Authorize]
        public Task<ActionResult<MessageResponse>> PostOperation1MultiAsync(MyRequest1 request
            , [FromServices] ITelemetryAdapter telemetry
            , [FromServices] IMessageBusAdapter bus)
        {

            return this.RunWithTelemetryAsync("COMMAND Operation1Multi",
                (state) =>
                {

                    var cmds = request.CreateFakeCommands(HttpContext.User.GetNameIdentifier());
                    return bus.PublishCommandsAsync(cmds, HttpContext.RequestAborted, state);

                },  _logger);



        }
    }
}