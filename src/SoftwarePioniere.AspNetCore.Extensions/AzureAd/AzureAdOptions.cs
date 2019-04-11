// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CheckNamespace
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace SoftwarePioniere.AspNetCore
{
    public class AzureAdOptions : ISwaggerClientOptions
    {
        public string TenantId { get; set; }
        public string Authority => $"https://login.microsoftonline.com/{TenantId}/";
        public string IssuerUrl => $"https://sts.windows.net/{TenantId}/";
        public string IssuerSigningKey { get; set; }
        public string Resource { get; set; }
        public string AdminGroupId { get; set; }
        public string UserGroupId { get; set; }
        public string SwaggerClientId { get; set; }
        public string SwaggerClientSecret => string.Empty;
        public string SwaggerAuthorizationUrl => $"{Authority}oauth2/authorize";
        public string SwaggerResource => Resource;
        public string ContextTokenAddPaths { get; set; }
    }
}


