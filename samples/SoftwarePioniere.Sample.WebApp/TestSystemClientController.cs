using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.AspNetCore;
using SoftwarePioniere.Sample.WebApp.Clients;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftwarePioniere.Sample.WebApp
{
    [Route("testsystemclient")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "test")]
    public class TestSystemClientController : ControllerBase
    {
        private readonly ILogger<TestSystemClientController> _logger;

        public TestSystemClientController(ILogger<TestSystemClientController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        [HttpGet("forbidden")]
        [SwaggerOperation(OperationId = "GetTestSystemClientForbidden")]
        public Task<ActionResult<ModelA>> GetForbidden(
            [FromServices] ITestSystemClient client)
        {
            return this.RunWithTelemetryAsync("QUERY GetForbidden", (state) => client.GetForbidden(), _logger);

        }

        [HttpGet("ok")]
        [SwaggerOperation(OperationId = "GetTestSystemClientOk")]
        public Task<ActionResult<ModelA>> GetOk([FromServices] ITestSystemClient client)
        {
            return this.RunWithTelemetryAsync("QUERY GetOk", (state) => client.GetForbidden(), _logger);

        }

        [HttpGet("nocontent")]
        [SwaggerOperation(OperationId = "GetTestSystemClientNoContent")]
        public Task<ActionResult<ModelA>> GetNoContent([FromServices] ITestSystemClient client)
        {
            return this.RunWithTelemetryAsync("QUERY GetNoContent", (state) => client.GetForbidden(), _logger);

        }

        [HttpGet("badrequest")]
        [SwaggerOperation(OperationId = "GetTestSystemClientBadRequest")]
        public Task<ActionResult<ModelA>> GetBadRequest([FromServices] ITestSystemClient client)
        {
            return this.RunWithTelemetryAsync("QUERY GetBadRequest", (state) => client.GetForbidden(), _logger);

        }


    }
}