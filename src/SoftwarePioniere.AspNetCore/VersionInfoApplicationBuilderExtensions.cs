using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SoftwarePioniere.AspNetCore
{
    public static class VersionInfoApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseVersionInfo(this IApplicationBuilder app, string baseRoute, Action<string> log)
        {
            {
                var url = string.Concat("/", baseRoute, "/version");
                log($"UseVersionInfo Version on Url: {url}");

                app.Map(url,
                    applicationBuilder =>
                    {
                        applicationBuilder.Run(async context =>
                        {
                            var assembly = Assembly.GetEntryAssembly();
                            var ret = (assembly ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                            context.Response.ContentType = "text/plain";
                            await context.Response.WriteAsync(ret).ConfigureAwait(false);

                        });
                    });
            }


            {
                var url = string.Concat("/", baseRoute, "/title");
                log($"UseVersionInfo Title on Url: {url}");

                app.Map(url,
                    applicationBuilder =>
                    {
                        applicationBuilder.Run(async context =>
                        {
                            var assembly = Assembly.GetEntryAssembly();
                            var ret = (assembly ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? assembly.GetName().Name;
                            context.Response.ContentType = "text/plain";
                            await context.Response.WriteAsync(ret).ConfigureAwait(false);

                        });
                    });
            }

            return app;
        }
    }
}
