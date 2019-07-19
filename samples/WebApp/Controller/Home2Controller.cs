using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApp.Controller
{
    [Route("api2")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "api2")]
    public class Home2Controller : ControllerBase
    {
        [HttpGet("x")]
        [SwaggerOperation(OperationId = "GetX2")]
        public ActionResult<string> GetX()
        {
            return "Hallo";
        }
    }
}