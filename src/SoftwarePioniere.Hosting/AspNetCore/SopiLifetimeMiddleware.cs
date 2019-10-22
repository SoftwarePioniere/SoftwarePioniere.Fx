using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Builder;

namespace SoftwarePioniere.Hosting.AspNetCore
{
    public class SopiLifetimeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly ISopiApplicationLifetime _lifetime;

        public SopiLifetimeMiddleware(RequestDelegate next, ILogger<SopiLifetimeMiddleware> logger, ISopiApplicationLifetime lifetime)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger;
            _lifetime = lifetime;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_lifetime.IsStarting || !_lifetime.IsStarted || _lifetime.Stopped.IsCancellationRequested)
            {
                var text = "SopiAppService ";

                if (_lifetime.IsStarting)
                    text = string.Concat(text, "starting");
                else if (!_lifetime.IsStarted)
                    text = string.Concat(text, "not started");
                else if (_lifetime.Stopped.IsCancellationRequested)
                    text = string.Concat(text, "commandhandlerstopped");

                _logger.LogWarning(text);

                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(text, Encoding.UTF8);

            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }

    public class SopiLifetimeActionFilter : IAsyncActionFilter
    {

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var lifetime = context.HttpContext.RequestServices.GetRequiredService<ISopiApplicationLifetime>();

            if (lifetime.IsStarted)
            {
                await next();
                return;
            }

            if (lifetime.IsStarting)
            {
                var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<LifetimeOptions>>().Value;
                if (context.HttpContext.Request.Method == "GET" && options.AllowGetWhileStarting)
                {
                    await next();
                    return;
                }
            }

            if (lifetime.IsStarting || !lifetime.IsStarted || lifetime.IsStopped)
            {

                var text = "SopiAppService ";

                if (lifetime.IsStarting)
                    text = string.Concat(text, "starting");
                else if (!lifetime.IsStarted)
                    text = string.Concat(text, "not started");
                else if (lifetime.IsStopped)
                    text = string.Concat(text, "stopped");

                var result = new ObjectResult(text) { StatusCode = (int)HttpStatusCode.ServiceUnavailable };
                context.Result = result;
                return;
            }


            await next();
        }
    }

    public static class SopiLifetimeMiddlewareExtensions
    {
        //public static IApplicationBuilder UseSopiLifetime(this IApplicationBuilder builder)
        //{
        //    return builder.UseMiddleware<SopiLifetimeMiddleware>();
        //}


        public static IApplicationBuilder UseSopiLifetimeEndpoint(this IApplicationBuilder app, string baseRoute)
        {
            var url = string.Concat("/", baseRoute, "/lifetime");
            Console.WriteLine("UseSopiLifetimeEndpoint on Url: {0}", url);

            app.Map(url,
                applicationBuilder =>
                {
                    applicationBuilder.Run(async context =>
                    {
                        var lifetime = app.ApplicationServices.GetRequiredService<ISopiApplicationLifetime>();
                        context.Response.ContentType = "text/plain";
                        if (lifetime.IsStarted)
                            await context.Response.WriteAsync("started", Encoding.UTF8);
                        if (lifetime.IsStarting)
                            await context.Response.WriteAsync("starting", Encoding.UTF8);
                        if (lifetime.IsStopped)
                            await context.Response.WriteAsync("stopped", Encoding.UTF8);


                    });
                });

            return app;
        }
    }
}
