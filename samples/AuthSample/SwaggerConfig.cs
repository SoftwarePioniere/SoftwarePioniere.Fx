using System.Collections.Generic;
using System.Linq;
using SoftwarePioniere.Extensions.AspNetCore;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.Swagger;

namespace AuthSample
{
    public static class SwaggerConfig
    {
        public static void TestApi(SopiSwaggerOptions c, ISwaggerClientOptions swaggerClientOptions)
        {
            c.Title = "Test Api";
            c.Docs = new[] { "api", "test" }.Select(apiKey => new SwaggerDocOptions { Name = apiKey, Title = $"{c.Title} [{apiKey}]", Url = $"/swagger/{apiKey}/swagger.json" }).ToArray();
            c.XmlFiles = new string[0];

            var scopes = new Dictionary<string, string>
            {
                {"admin", "admin access"}
            };

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
        }
    }
}
