using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.AspNetCore;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.ReadModel;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftwarePioniere.Sample.WebApp
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "qry1")]
    [Authorize]
    public class MyQueryController : ControllerBase
    {
        private readonly ILogger _logger;

        public MyQueryController(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger(GetType());
        }

        [HttpGet("liste")]
        [SwaggerOperation(OperationId = "GetListe")]
        public Task<ActionResult<FakeEntity[]>> GetListeAsync(
            [FromServices] MyQueryService queryService
            , [FromServices] ITelemetryAdapter telemetry

            )
        {
            return this.RunWithTelemetryAsync("QUERY GetListe",
                (state) => queryService.GetListeAsync(HttpContext.User, state)
                ,  _logger);


            //var state =telemetry.CreateState(HttpContext);
            //return await queryService.GetListeAsync(HttpContext.User, state);

            //return this.QueryAsync("GetListe"
            //    , (state) => queryService.GetListeAsync(HttpContext.User, state)
            //    , _logger);

        }

        [HttpGet("liste2")]
        [SwaggerOperation(OperationId = "GetListe2")]
        public Task<ActionResult<FakeEntity[]>> GetListe2Async(
            [FromServices] MyQueryService queryService
            , [FromServices] ITelemetryAdapter telemetry
            )
        {
            return this.RunWithTelemetryAsync("QUERY GetListe2",
                async (state) =>
                {
                    await ThrowAuthAsync(HttpContext.User);
                    return await queryService.GetListeAsync(HttpContext.User, state);

                },  _logger);

            //return this.DoGet(() => queryService.GetListeAsync(User, null),
            //    nameof(GetListe2Async), _logger);

        }

        private Task ThrowAuthAsync(ClaimsPrincipal httpContextUser)
        {
            throw new AuthenticationException("nur mal so " + httpContextUser.GetNameIdentifier());
        }
    }

    public class MyQueryService
    {
        private readonly ICacheAdapter _cache;
        private readonly IEntityStore _entityStore;
        private readonly ILogger _logger;

        public MyQueryService(ILoggerFactory loggerFactory, ICacheAdapter cache, IEntityStore entityStore)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _entityStore = entityStore ?? throw new ArgumentNullException(nameof(entityStore));
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public Task<FakeEntity[]> GetListeAsync(ClaimsPrincipal user, IDictionary<string, string> state)
        {
            var items = _cache.CacheLoad(() => _entityStore.LoadItemsAsync<FakeEntity>(),
                CacheKeys.Create<FakeEntity>("liste"),
                logger: _logger
            );

            return items;
        }
    }
}
