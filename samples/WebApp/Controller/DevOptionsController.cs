using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SoftwarePioniere.Builder;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApp.Controller
{
    [Route("devoptions")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "api")]
    public class DevOptionsController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = "GetDevOptions")]
        public ActionResult<string> GetDevOptions(
           [FromServices] IOptionsSnapshot<DevOptions> options
           , [FromServices] IHostingEnvironment environment)
        {
            if (!environment.IsDevelopment())
                return BadRequest("only in development enviroment");

            var o = options.Value;
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }

        [HttpPost("toggle/getwithbadrequest")]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = "PostToggleDevOptionsGetWithBadRequest")]
        public ActionResult<string> PostToggleDevOptionsGetWithBadRequest(
            [FromServices] DevOptionsConfigurationProvider provider
            , [FromServices] IHostingEnvironment environment

            )
        {
            if (!environment.IsDevelopment())
                return BadRequest("only in development enviroment");


            provider.SetOption(x => x.GetWithBadRequest = !x.GetWithBadRequest);
            return $"GetWithBadRequest: {provider.DevOptions.GetWithBadRequest}";
        }

        [HttpPost("toggle/postwithbadrequest")]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = "PostToggleDevOptionsPostWithBadRequest")]
        public ActionResult<string> PostToggleDevOptionsPostWithBadRequest(
            [FromServices] DevOptionsConfigurationProvider provider
            , [FromServices] IHostingEnvironment environment
        )
        {
            if (!environment.IsDevelopment())
                return BadRequest("only in development enviroment");

            provider.SetOption(x => x.PostWithBadRequest = !x.PostWithBadRequest);
            return $"PostWithBadRequest: {provider.DevOptions.PostWithBadRequest}";
        }

        [HttpPost("toggle/raisecommandfailed")]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = "PostToggleDevOptionsRaiseCommandFailed")]
        public ActionResult<string> PostToggleDevOptionsRaiseCommandFailed(
            [FromServices] DevOptionsConfigurationProvider provider
            , [FromServices] IHostingEnvironment environment)
        {
            if (!environment.IsDevelopment())
                return BadRequest("only in development enviroment");

            provider.SetOption(x => x.RaiseCommandFailed = !x.RaiseCommandFailed);
            return $"RaiseCommandFailed: {provider.DevOptions.RaiseCommandFailed}";
        }
    }
}
