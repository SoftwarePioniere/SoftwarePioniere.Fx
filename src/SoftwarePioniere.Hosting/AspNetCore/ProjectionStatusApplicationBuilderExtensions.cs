using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SoftwarePioniere.Projections;

namespace SoftwarePioniere.Hosting.AspNetCore
{
    public static class ProjectionStatusApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseProjectionStatusEndpoint(this IApplicationBuilder app, string baseRoute, Action<string> log)
        {
            var url = string.Concat("/", baseRoute, "/status");
            log($"UseProjectionStatusEndpoint on Url: {url}");


            app.Map(url,
                applicationBuilder =>
                {
                    applicationBuilder.Run(async context =>
                    {
                        var registry = app.ApplicationServices.GetRequiredService<IProjectorRegistry>();
                        var status = await registry.GetStatusAsync().ConfigureAwait(false);
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(status), Encoding.UTF8).ConfigureAwait(false);

                    });
                });

            return app;
        }
    }
}
