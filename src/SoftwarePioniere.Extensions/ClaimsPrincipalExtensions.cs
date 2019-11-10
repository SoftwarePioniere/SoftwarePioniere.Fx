using System;
using System.Linq;
using System.Security.Claims;

namespace SoftwarePioniere
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetNameIdentifier(this ClaimsPrincipal user, bool throwErrorOnMissingClaim = true)
        {
            const string claim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
            var value = user?.Claims?.FirstOrDefault(c => c.Type == claim)?.Value;

            if (string.IsNullOrEmpty(value))
            {
                if (throwErrorOnMissingClaim)
                    throw new InvalidOperationException("no id claim in token: " + claim);
                else
                    return "ANONYMOUS";
            }

            return value;
        }

        public static bool IsClientCredential(this ClaimsPrincipal user)
        {
            const string claim = "gty";

            var value = user.Claims.FirstOrDefault(c => c.Type == claim)?.Value;

            if (!string.IsNullOrEmpty(value))
            {
                return string.Equals(value, "client-credentials", StringComparison.InvariantCultureIgnoreCase);
            }

            const string claim2 = "appidacr";
            var value2 = user.Claims.FirstOrDefault(c => c.Type == claim2)?.Value;

            if (!string.IsNullOrEmpty(value2))
            {
                return string.Equals(value2, "1", StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        public static ClaimsPrincipal CreateClientCredential()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("gty", "client-credentials"),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier","SYSTEM")
            }));
            return user;
        }
    }
}
