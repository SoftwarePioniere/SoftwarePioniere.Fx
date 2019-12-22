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
            var items = await queryService.GetListeAsync(HttpContext.User);
            return Ok(items);

        }

        [HttpGet("liste2")]
        [SwaggerOperation(OperationId = "GetListe2")]
        public async Task<ActionResult<IEnumerable<FakeEntity>>> GetListe2Async(
            [FromServices] MyQueryService queryService
            )
        {
            Logger.LogInformation("Liste2");
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
