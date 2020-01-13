using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace SoftwarePioniere.Extensions.AspNetCore.Swagger
{
    public class SopiSwaggerOptions
    {
        public string[] XmlFiles { get; set; }
        public OpenApiSecurityScheme OAuth2Scheme { get; set; }
        public string AuthorizationUrl { get; set; }
        public Dictionary<string, string> Scopes { get; set; }
        public Dictionary<string, string> OAuthAdditionalQueryStringParams { get; set; }
        public string OAuthClientId { get; set; }
        public string OAuthClientSecret { get; set; }
        public SwaggerDocOptions[] Docs { get; set; }
        public SwaggerDocOptions[] UiDocs { get; set; }
        public bool UseSwaggerUi { get; set; } = true;
        public bool ReadOnlyUi { get; set; }
        public string Title { get; set; }
        public string Doc { get; set; }
        public string RouteTemplate { get; set; }
        public string ServiceName { get; set; }
    }

    public class SwaggerDocOptions
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
    }

    public class SopiSwaggerAuthOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public Uri AuthorizationUrl { get; set; }
        public string Resource { get; set; }
    }
}