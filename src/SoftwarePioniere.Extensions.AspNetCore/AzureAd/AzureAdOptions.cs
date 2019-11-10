// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace SoftwarePioniere.Extensions.AspNetCore.AzureAd
{
    public class AzureAdOptions
    {
        public string TenantId { get; set; }

        public string IssuerSigningKey { get; set; }
        public string Resource { get; set; }
        public string AdminGroupId { get; set; }
        public string UserGroupId { get; set; }
        public string SwaggerClientId { get; set; }
        public string SwaggerClientSecret => string.Empty;
        public string ContextTokenAddPaths { get; set; }
        public string NameClaimType { get; set; } = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    }
}


