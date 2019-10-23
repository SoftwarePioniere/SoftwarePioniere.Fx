using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using WebApp.Clients;

namespace WebApp.Controller
{
    [Route("testsystemclient")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "test")]
    public class TestSystemClientController : MyControllerBase
    {

        [HttpGet("forbidden")]
        [SwaggerOperation(OperationId = "GetTestSystemClientForbidden")]
        public async Task<ActionResult<ModelA>> GetForbidden(
            [FromServices] ITestSystemClient client)
        {
            return await client.GetForbidden();

        }

        [HttpGet("ok")]
        [SwaggerOperation(OperationId = "GetTestSystemClientOk")]
        public async Task<ActionResult<ModelA>> GetOk([FromServices] ITestSystemClient client)
        {
            return await client.GetOk();

        }

        [HttpGet("nocontent")]
        [SwaggerOperation(OperationId = "GetTestSystemClientNoContent")]
        public async Task<ActionResult<ModelA>> GetNoContent([FromServices] ITestSystemClient client)
        {
            return await client.GetNoContent();

        }

        [HttpGet("badrequest")]
        [SwaggerOperation(OperationId = "GetTestSystemClientBadRequest")]
        public async Task<ActionResult<ModelA>> GetBadRequest([FromServices] ITestSystemClient client)
        {
            return await client.GetBadRequest();

        }


        public TestSystemClientController(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }
    }
}