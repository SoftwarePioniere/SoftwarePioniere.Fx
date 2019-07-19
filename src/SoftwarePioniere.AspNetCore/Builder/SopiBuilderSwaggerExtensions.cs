using System;
using System.Collections.Generic;
using System.Linq;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.Swagger;

// ReSharper disable once CheckNamespace
namespace SoftwarePioniere.AspNetCore.Builder
{
    public static class SopiBuilderSwaggerExtensions
    {
        public static ISopiBuilder AddSwaggerForMultipleServices(this ISopiBuilder builder, string titleName, string baseRoute, string serviceName, string[] apiKeys
        , Action<SopiSwaggerOptions> configureOptions = null)
        {
            var swaggerClientOptions = builder.GetSwaggerClientOptions();

            builder.Services.AddMySwagger(c =>
            {
                c.Title = $"{titleName} {serviceName}";
                c.Doc = apiKeys[0];

                var docs = apiKeys.Select(apiKey => new SwaggerDocOptions { Name = apiKey, Title = c.Title, Url = $"/{baseRoute}/{serviceName}/swagger/{apiKey}.json" }).ToArray();
                c.Docs = docs;
                c.RouteTemplate = $"{baseRoute}/{serviceName}" + "/swagger/{documentName}.json";

                var scopes = new Dictionary<string, string>();
                //{
                //    {"admin", "admin access"}
                //};

                c.OAuth2Scheme = new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = swaggerClientOptions.SwaggerAuthorizationUrl,
                    Scopes = scopes
                };

                c.OAuthAdditionalQueryStringParams = new Dictionary<string, string>
                {
                    {"resource", swaggerClientOptions.SwaggerResource},
                    {"Audience", swaggerClientOptions.SwaggerResource}
                };

                c.OAuthClientId = swaggerClientOptions.SwaggerClientId;
                c.OAuthClientSecret = swaggerClientOptions.SwaggerClientSecret;

                configureOptions?.Invoke(c);
            });

            return builder;
        }

        public static ISopiBuilder AddSwaggerForSingleService(this ISopiBuilder builder, string apiKey, string baseRoute, string serviceName
        , Action<SopiSwaggerOptions> configureOptions = null)
        {
            var swaggerClientOptions = builder.GetSwaggerClientOptions();

            builder.Services.AddMySwagger(c =>
            {
                c.Title = $"{apiKey} {serviceName}";
                c.Doc = apiKey;
                c.Docs = new[]
                {
                    new SwaggerDocOptions{Name = apiKey , Title  = c.Title,  Url=$"/{baseRoute}/{serviceName}/swagger/{apiKey}.json" }
                };
                c.RouteTemplate = $"{baseRoute}/{serviceName}" + "/swagger/{documentName}.json";
                c.ServiceName = serviceName;

                var scopes = new Dictionary<string, string>();
                //{
                //    {"admin", "admin access"}
                //};

                c.OAuth2Scheme = new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = swaggerClientOptions.SwaggerAuthorizationUrl,
                    Scopes = scopes
                };

                c.OAuthAdditionalQueryStringParams = new Dictionary<string, string>
                {
                    {"resource", swaggerClientOptions.SwaggerResource},
                    {"Audience", swaggerClientOptions.SwaggerResource}
                };

                c.OAuthClientId = swaggerClientOptions.SwaggerClientId;
                c.OAuthClientSecret = swaggerClientOptions.SwaggerClientSecret;

                configureOptions?.Invoke(c);
            });

            return builder;
        }


    }
}
