using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftwarePioniere;
using SoftwarePioniere.Messaging;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApp.Controller
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "cmd1")]
    public class MyCommandController : MyControllerBase
    {
      

        [HttpPost("op2")]
        [SwaggerOperation(OperationId = "PostOperation2")]
        [Authorize]
        public async Task<ActionResult<MessageResponse>> PostOperation2Async(MyRequest1 request, [FromServices] IMessageBusAdapter bus
            )
        {

            var cmd = request.CreateFakeCommand(HttpContext.User.GetNameIdentifier());
            return await bus.PublishCommandAsync(cmd, HttpContext.RequestAborted);

        }

        [HttpPost("op1")]
        [SwaggerOperation(OperationId = "PostOperation1")]
        [Authorize]
        public async Task<ActionResult<MessageResponse>> PostOperation1Async(MyRequest1 request, [FromServices] IMessageBusAdapter bus)
        {

            await ThrowAuthAsync(HttpContext.User);
            var cmd = request.CreateFakeCommand(HttpContext.User.GetNameIdentifier());
            return await bus.PublishCommandAsync(cmd, HttpContext.RequestAborted);

        }

        private Task ThrowAuthAsync(ClaimsPrincipal httpContextUser)
        {
            throw new AuthenticationException($"nur mal so, {httpContextUser.GetNameIdentifier()}");
        }

        [HttpPost("op1/multi")]
        [SwaggerOperation(OperationId = "PostOperation1Multi")]
        [Authorize]
        public async Task<ActionResult<MessageResponse>> PostOperation1MultiAsync(MyRequest1 request, [FromServices] IMessageBusAdapter bus)
        {

            var cmds = request.CreateFakeCommands(HttpContext.User.GetNameIdentifier());
            return await bus.PublishCommandsAsync(cmds, HttpContext.RequestAborted);

        }

        public MyCommandController(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }
    }
}