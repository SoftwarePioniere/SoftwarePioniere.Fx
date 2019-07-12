using Microsoft.AspNetCore.Authorization;

namespace SoftwarePioniere.Extensions.AspNetCore
{
    public class AuthorizeAdminAttribute : AuthorizeAttribute
    {
        public AuthorizeAdminAttribute() : base(PolicyConstants.IsAdminPolicy)
        {

        }
    }
}