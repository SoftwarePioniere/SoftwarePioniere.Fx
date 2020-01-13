using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;

namespace AuthSample
{
    public class SwaggerConfig : IConfigureOptions<SopiSwaggerOptions>
    {
        private readonly SopiSwaggerAuthOptions _auth;

        public SwaggerConfig(IOptions<SopiSwaggerAuthOptions> auth)
        {
            _auth = auth.Value;
        }

        public void Configure(SopiSwaggerOptions options)
        {
            options.Title = "Test Api";
            options.Docs = new[]
            {
                "api", "test"
            }
            .Select(apiKey => new SwaggerDocOptions
            {
                Name = apiKey,
                Title = $"{options.Title} [{apiKey}]",
                Url = $"/swagger/{apiKey}/swagger.json"
            }).ToArray();

            options.XmlFiles = new string[0];

            options.Scopes = new Dictionary<string, string>
                {
                    {"admin", "admin access"}
                };

            options.OAuth2Scheme = new OpenApiSecurityScheme()
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        Scopes = options.Scopes,
                        AuthorizationUrl = _auth.AuthorizationUrl
                    }
                }
            };

            options.OAuthAdditionalQueryStringParams = new Dictionary<string, string>
                {
                    {"resource", _auth.Resource},
                    {"Audience", _auth.Resource}
                };

            options.OAuthClientId = _auth.ClientId;
            options.OAuthClientSecret = _auth.ClientSecret;
        }

    }

}

