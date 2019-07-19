using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApp.Clients;

namespace WebApp.Controller
{
    [Route("testclient")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "test")]
    public class TestClientController : ControllerBase
    {

        [HttpGet("forbidden")]
        [SwaggerOperation(OperationId = "GetForbidden")]
        public async Task<ActionResult<ModelA>> GetForbidden()
        {
            await Task.Delay(0);
            return Forbid();
        }

        [HttpGet("ok")]
        [SwaggerOperation(OperationId = "GetOk")]
        public async Task<ActionResult<ModelA>> GetOk()
        {
            await Task.Delay(0);
            return new ModelA
            {
                Text = "alles gut"
            };
        }

        [HttpGet("okauth")]
        [Authorize]
        [SwaggerOperation(OperationId = "GetOkAuth")]
        public async Task<ActionResult<ModelA>> GetOkAuth()
        {
            await Task.Delay(0);
            return new ModelA
            {
                Text = "alles gut"
            };
        }

        [HttpGet("nocontent")]
        [SwaggerOperation(OperationId = "GetNoContent")]
        public async Task<ActionResult<ModelA>> GetNoContent()
        {
            await Task.Delay(0);
            return NoContent();
        }

        [HttpGet("badrequest")]
        [SwaggerOperation(OperationId = "GetBadRequest")]
        public async Task<ActionResult<ModelA>> GetBadRequest()
        {
            await Task.Delay(0);
            return BadRequest("da stimmt was nicht");
        }


    }
}