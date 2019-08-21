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
        public static IApplicationBuilder UseProjectionStatusEndpoint(this IApplicationBuilder app, string baseRoute)
        {
            app.Map(string.Concat("/", baseRoute, "/status"),
                applicationBuilder =>
                {
                    applicationBuilder.Run(async context =>
                    {
                        var registry = app.ApplicationServices.GetRequiredService<IProjectorRegistry>();
                        var status = await registry.GetStatusAsync();
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(status), Encoding.UTF8);

                    });
                });

            return app;
        }
    }
}
