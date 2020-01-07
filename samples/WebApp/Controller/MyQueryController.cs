using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftwarePioniere;
using SoftwarePioniere.FakeDomain;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApp.Controller
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "qry1")]
    [Authorize]
    public class MyQueryController : MyControllerBase
    {


        [HttpGet("liste")]
        [SwaggerOperation(OperationId = "GetListe")]
        public async Task<ActionResult<IEnumerable<FakeEntity>>> GetListeAsync(
            [FromServices] MyQueryService queryService

            )
        {
            Logger.LogInformation("GetListe");
            var items = await queryService.GetListeAsync(HttpContext.User);
            return Ok(items);

        }

        [HttpGet("liste2")]
        [SwaggerOperation(OperationId = "GetListe2")]
        public async Task<ActionResult<IEnumerable<FakeEntity>>> GetListe2Async(
            [FromServices] MyQueryService queryService

        )
        {
            Logger.LogInformation("GetListe2Async");
            var items = await queryService.GetListe2Async(HttpContext.User);
            return Ok(items);

        }

        [HttpGet("liste3")]
        [SwaggerOperation(OperationId = "GetListe3")]
        public async Task<ActionResult<IEnumerable<FakeEntity>>> GetListe3Async(
            [FromServices] MyQueryService queryService

        )
        {
            Logger.LogInformation("GetListe3Async");
            var items = await queryService.GetListe3Async(HttpContext.User, HttpContext.RequestAborted);
            return Ok(items);

        }
        [HttpGet("liste_throw_auth")]
        [SwaggerOperation(OperationId = "GetListeThrowAuthAsync")]
        public async Task<ActionResult<IEnumerable<FakeEntity>>> GetListeThrowAuthAsyncAsync(
            [FromServices] MyQueryService queryService
            )
        {
            Logger.LogInformation("GetListeThrowAuthAsyncAsync");
            await ThrowAuthAsync(HttpContext.User);
            var items = await queryService.GetListeAsync(HttpContext.User);
            return Ok(items);


        }

        private Task ThrowAuthAsync(ClaimsPrincipal httpContextUser)
        {
            throw new AuthenticationException("nur mal so " + httpContextUser.GetNameIdentifier());
        }

        public MyQueryController(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }
    }
}
