using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SoftwarePioniere.Extensions.AspNetCore.Swagger
{
    public static class SwaggerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMySwagger(this IApplicationBuilder app, Action<SopiSwaggerOptions> setupAction)
        {
            var options = new SopiSwaggerOptions();
            setupAction(options);
            return app.UseMySwagger(options);
        }

        public static IApplicationBuilder UseMySwagger(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<SopiSwaggerOptions>>().Value;
            return app.UseMySwagger(options);
        }

        public static IApplicationBuilder UseMySwagger(this IApplicationBuilder app, SopiSwaggerOptions options)
        {

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);

                if (!string.IsNullOrEmpty(options.RouteTemplate))
                {
                    c.RouteTemplate = options.RouteTemplate;
                }
            });

            if (options.UseSwaggerUi)
            {
                app.UseSwaggerUI(c =>
                {
                    if (options.Docs != null)
                    {
                        foreach (var doc in options.Docs)
                        {
                            var name = doc.Name;
                            if (!string.IsNullOrEmpty(options.ServiceName))
                            {
                                name = $"{doc.Name}-{options.ServiceName}";
                            }

                            c.SwaggerEndpoint(doc.Url, name);
                        }
                    }
                    else
                    {
                        var doc = "api";
                        if (!string.IsNullOrEmpty(options.Doc))
                        {
                            doc = options.Doc;
                        }

                        c.SwaggerEndpoint($"/swagger/{doc}/swagger.json", doc);
                    }

                    if (options.UiDocs != null)
                    {
                        foreach (var doc in options.UiDocs)
                        {
                            c.SwaggerEndpoint(doc.Url, doc.Name);
                        }
                    }

                    if (options.OAuthAdditionalQueryStringParams != null)
                    {
                        c.OAuthAdditionalQueryStringParams(options.OAuthAdditionalQueryStringParams);
                    }

                    if (!string.IsNullOrEmpty(options.OAuthClientId))
                    {
                        c.OAuthClientId(options.OAuthClientId);
                    }

                    if (!string.IsNullOrEmpty(options.OAuthClientSecret))
                    {
                        c.OAuthClientSecret(options.OAuthClientSecret);
                    }
                });
            }

            return app;
        }

    }

}
