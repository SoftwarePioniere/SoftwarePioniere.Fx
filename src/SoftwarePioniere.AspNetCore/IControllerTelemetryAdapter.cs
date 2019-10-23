//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using SoftwarePioniere.Telemetry;

//namespace SoftwarePioniere.AspNetCore
//{
//    public interface IControllerTelemetryAdapter : ITelemetryAdapter
//    {
//        Task<ActionResult<T>> RunWithActionResultAsync<T>(string operationName
//            , Func<IDictionary<string, string>, Task<T>> runx
//            , HttpContext httpContext
//            , ILogger logger
//        );
//    }

//    public static class ControllerExtensions
//    {
//        public static Task<ActionResult<T>> RunWithTelemetryAsync<T>(this ControllerBase controller, string operationName
//            , Func<IDictionary<string, string>, Task<T>> runx, ILogger logger)
//        {
//            var adapter = controller.HttpContext.RequestServices.GetRequiredService<IControllerTelemetryAdapter>();
//            return adapter.RunWithActionResultAsync(operationName, runx, controller.HttpContext, logger);

//        }
//    }
//}
