using System;
using System.Linq;
using System.Threading.Tasks;
using Foundatio.Caching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.ReadModel;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApp.Controller
{
    [Route("api")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "api")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("claims")]
        [SwaggerOperation(OperationId = "GetIdentityClaims")]
        [Authorize]
        public ActionResult<KeyValueItem[]> GetIdentityClaims()
        {

            _logger.LogDebug("GetIdentityClaims");
            var items = User.Claims.Select(x => new KeyValueItem
            {
                Key = x.Type,
                Value = x.Value
            }).ToArray();

            return Ok(items);
        }

        [HttpGet("x")]
        [SwaggerOperation(OperationId = "GetX")]
        public ActionResult<string> GetX()
        {
            _logger.LogDebug("a");
            return "Hallo";
        }

        [HttpPost("stop")]
        [SwaggerOperation(("PostStop"))]
        public ActionResult<string> PostStop(
            [FromServices] ISopiApplicationLifetime lifetime)
        {
            lifetime.Stop();
            return "ok";
        }

        [HttpPost("postx")]
        [SwaggerOperation(("PostX"))]
        public async Task<ActionResult<string>> Postx(
            [FromServices] IEntityStore ies,
            [FromServices] ICacheClient cc)
        {
            await cc.SetAsync("hallo", "test");
            return (await cc.GetAsync<string>("hallo")).Value;
        }

        //[HttpPost("postmsg")]
        //[SwaggerOperation(OperationId = "PostMessage")]
        //public async Task<ActionResult<MessageResponse>> PostMessage([FromServices] CommandService service)
        //{
        //    var cmd = new DummyCommand(Guid.NewGuid(), DateTime.UtcNow, "userid", -1, HttpContext.TraceIdentifier);
        //    return await service.SendToBus<DummyCommand>(this, _logger, cmd, TimeSpan.FromMinutes(1));
        //}

        //[HttpPost("postmsg2")]
        //[SwaggerOperation(OperationId = "PostMessage2")]
        //public async Task<ActionResult<MessageResponse>> PostMessage2(
        //    [FromServices] IMessageBus bus)
        //{

        //    return await this.DoPost(async () =>
        //    {
        //        var cmd = new DummyCommand(Guid.NewGuid(), DateTime.UtcNow, "userid", -1, HttpContext.TraceIdentifier);
        //        return await this.SendToBus<DummyCommand>(bus, _logger, cmd);
        //    }, "PostMessage2", _logger);

        //}

        //[HttpPost("postmsg3")]
        //[SwaggerOperation(OperationId = "PostMessage3")]
        //public async Task<ActionResult<MessageResponse>> PostMessage3([FromServices] CommandService service)
        //{
        //    return await service.DoPost(this, async () =>
        //    {
        //        var cmd = new DummyCommand(Guid.NewGuid(), DateTime.UtcNow, "userid", -1, HttpContext.TraceIdentifier);
        //        return await service.SendToBus<DummyCommand>(this, _logger, cmd);
        //    }, "PostMessage3", _logger);

        //}

        [HttpGet("devoptions")]
        [SwaggerOperation(OperationId = "GetDevOptions")]
        public ActionResult<DevOptions> GetDevOptions([FromServices] IOptions<DevOptions> options)
        {

            return options.Value;
        }

        [HttpGet("devoptions2")]
        [SwaggerOperation(OperationId = "GetDevOptions2")]
        public ActionResult<DevOptions> GetDevOptions2([FromServices] IOptionsMonitor<DevOptions> monitor)
        {
            return monitor.CurrentValue;
        }


        [HttpGet("devoptions3")]
        [SwaggerOperation(OperationId = "GetDevOptions3")]
        public ActionResult<DevOptions> GetDevOptions3([FromServices] IOptionsSnapshot<DevOptions> snapshot)
        {
            return snapshot.Value;
        }

        //[HttpPost("devoptions")]
        //[SwaggerOperation(OperationId = "PostDevOptions")]
        //public ActionResult PostDevOptions([FromBody] DevOptions options
        //     , [FromServices] DevOptionsConfigurationProvider prov
        //    // , [FromServices] MemoryConfigurationProvider prov
        //    , [FromServices] IOptions<Fliegel365Options> fliegel365Options


        //    )
        //{

        //    if (!fliegel365Options.Value.AllowDevMode)
        //        return BadRequest("DevMode not allowed");

        //    prov.SetDevOptions(options);

        //    return Ok(options);
        //}



    }
}
