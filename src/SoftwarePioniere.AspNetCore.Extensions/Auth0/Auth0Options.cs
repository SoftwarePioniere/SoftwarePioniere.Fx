// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CheckNamespace
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace SoftwarePioniere.AspNetCore
{
    public class Auth0Options : ISwaggerClientOptions
    {
        public string TenantId { get; set; }
        public string Domain => $"https://{TenantId}/";
        public string Audience { get; set; }
        public string AdminGroupId { get; set; }
        public string UserGroupId { get; set; }
        public string GroupClaimType { get; set; } = "http://softwarepioniere.de/groups";
        public string SwaggerClientId { get; set; }
        public string SwaggerClientSecret { get; set; }
        public string SwaggerAuthorizationUrl => $"{Domain}authorize";
        public string SwaggerResource => Audience;
        public string ContextTokenAddPaths { get; set; }
    }
}
