using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SoftwarePioniere.AspNetCore
{
    public static class VersionInfoApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseVersionInfo(this IApplicationBuilder app, string baseRoute)
        {
            app.Map(string.Concat("/", baseRoute, "/version"),
                applicationBuilder =>
                {
                    applicationBuilder.Run(async context =>
                    {
                        var assembly = Assembly.GetEntryAssembly();
                        var ret = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(ret);

                    });
                });

            app.Map(string.Concat("/", baseRoute, "/title"),
                applicationBuilder =>
                {
                    applicationBuilder.Run(async context =>
                    {
                        var assembly = Assembly.GetEntryAssembly();
                        var ret = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? assembly.GetName().Name;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(ret);

                    });
                });

            return app;
        }
    }
}
