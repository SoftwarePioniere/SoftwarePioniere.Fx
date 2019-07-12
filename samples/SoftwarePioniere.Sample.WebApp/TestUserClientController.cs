using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SoftwarePioniere.Sample.WebApp.Clients;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftwarePioniere.Sample.WebApp
{
    [Route("testuserclient")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "test")]
    public class TestUserClientController : ControllerBase
    {

        [HttpGet("forbidden")]
        [SwaggerOperation(OperationId = "GetTestUserClientForbidden")]
        public async Task<ActionResult<ModelA>> GetForbidden([FromServices] ITestUserClient client)
        {
            return await client.GetForbidden(User);
        }

        [HttpGet("ok")]
        [SwaggerOperation(OperationId = "GetTestUserClientOk")]
        public async Task<ActionResult<ModelA>> GetOk([FromServices] ITestUserClient client)
        {
            return await client.GetOk(User);
        }

        [HttpGet("nocontent")]
        [SwaggerOperation(OperationId = "GetTestUserClientNoContent")]
        public async Task<ActionResult<ModelA>> GetNoContent([FromServices] ITestUserClient client)
        {
            return await client.GetNoContent(User);
        }

        [HttpGet("badrequest")]
        [SwaggerOperation(OperationId = "GetTestUserClientBadRequest")]
        public async Task<ActionResult<ModelA>> GetBadRequest([FromServices] ITestUserClient client)
        {
            return await client.GetBadRequest(User);
        }


    }
}