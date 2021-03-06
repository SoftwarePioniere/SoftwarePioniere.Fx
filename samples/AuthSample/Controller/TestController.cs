﻿using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthSample.Controller
{

    [Route("api/test")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "test")]
    public class TestController : ControllerBase
    {
       
        [HttpGet("claims")]
        [Authorize]
        [SwaggerOperation(OperationId = "GetIdentityClaims")]
        public ActionResult<ClaimInfo[]> GetClaims()
        {
            return User.Claims.Select(c => new ClaimInfo { Type = c.Type, Value = c.Value }).ToArray();
        }

        [HttpGet("claims/admin")]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "GetIdentityClaimsAdmin")]
        public ActionResult<ClaimInfo[]> GetClaimsAdmin()
        {
            return User.Claims.Select(c => new ClaimInfo { Type = c.Type, Value = c.Value }).ToArray();
        }
    }
}
