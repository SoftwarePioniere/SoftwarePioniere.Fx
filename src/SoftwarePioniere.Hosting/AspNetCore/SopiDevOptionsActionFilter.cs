using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Builder;

namespace SoftwarePioniere.Hosting.AspNetCore
{
    public class SopiDevOptionsActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var env = context.HttpContext.RequestServices.GetRequiredService<IHostingEnvironment>();

            if (env.IsDevelopment())
            {
                var options = context.HttpContext.RequestServices.GetRequiredService<IOptionsSnapshot<DevOptions>>().Value;

                if (context.HttpContext.Request.Method == "GET" && options.GetWithBadRequest && !context.HttpContext.Request.Path.ToString().Contains("devoptions"))
                {
                    var text = "DevOptions Get with Bad Request";
                    var result = new ObjectResult(text) { StatusCode = (int)HttpStatusCode.BadRequest };
                    context.Result = result;
                    return;
                }

                if (context.HttpContext.Request.Method == "POST" && options.PostWithBadRequest && !context.HttpContext.Request.Path.ToString().Contains("devoptions"))
                {
                    var text = "DevOptions Post with Bad Request";
                    var result = new ObjectResult(text) { StatusCode = (int)HttpStatusCode.BadRequest };
                    context.Result = result;
                    return;

                }
            }

            await next();
        }
    }
}