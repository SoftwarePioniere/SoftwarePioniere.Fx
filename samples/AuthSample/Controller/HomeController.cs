﻿using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthSample.Controller
{
    [Route("api/home")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "api")]
    public class HomeController : ControllerBase
    {    
        [HttpGet("info")]
        [SwaggerOperation(OperationId = "GetApiInfo")]
        public ActionResult<ApiInfo> GetApiInfo()
        {
            var assembly = Assembly.GetEntryAssembly();

            if (assembly == null)
                return new ApiInfo();

            return new ApiInfo
            {
                Title1 = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? assembly.GetName().Name,
                Version1 = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion,
            };
        }
    }
}