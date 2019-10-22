using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SoftwarePioniere.AspNetCore
{
    public static class VersionInfoApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseVersionInfo(this IApplicationBuilder app, string baseRoute)
        {
            {
                var url = string.Concat("/", baseRoute, "/version");
                Console.WriteLine("UseVersionInfo Version on Url: {0}", url);

                app.Map(url,
                    applicationBuilder =>
                    {
                        applicationBuilder.Run(async context =>
                        {
                            var assembly = Assembly.GetEntryAssembly();
                            var ret = (assembly ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                            context.Response.ContentType = "text/plain";
                            await context.Response.WriteAsync(ret);

                        });
                    });
            }


            {
                var url = string.Concat("/", baseRoute, "/title");
                Console.WriteLine("UseVersionInfo Title on Url: {0}", url);

                app.Map(url,
                    applicationBuilder =>
                    {
                        applicationBuilder.Run(async context =>
                        {
                            var assembly = Assembly.GetEntryAssembly();
                            var ret = (assembly ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? assembly.GetName().Name;
                            context.Response.ContentType = "text/plain";
                            await context.Response.WriteAsync(ret);

                        });
                    });
            }

            return app;
        }
    }
}
