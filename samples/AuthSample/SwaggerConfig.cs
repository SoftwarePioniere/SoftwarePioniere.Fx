using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.Swagger;

namespace AuthSample
{
    public class SwaggerConfig : IConfigureOptions<SopiSwaggerOptions>
    {
        private readonly SopiSwaggerClientOptions _swaggerClientOptions;

        public SwaggerConfig(IOptions<SopiSwaggerClientOptions> cwco)
        {
            _swaggerClientOptions = cwco.Value;
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

            var scopes = new Dictionary<string, string>
                {
                    {"admin", "admin access"}
                };

            options.OAuth2Scheme = new OAuth2Scheme
            {
                Type = "oauth2",
                Flow = "implicit",
                AuthorizationUrl = _swaggerClientOptions.AuthorizationUrl,
                Scopes = scopes
            };

            options.OAuthAdditionalQueryStringParams = new Dictionary<string, string>
                {
                    {"resource", _swaggerClientOptions.Resource},
                    {"Audience", _swaggerClientOptions.Resource}
                };

            options.OAuthClientId = _swaggerClientOptions.ClientId;
            options.OAuthClientSecret = _swaggerClientOptions.ClientSecret;
        }

    }

}

