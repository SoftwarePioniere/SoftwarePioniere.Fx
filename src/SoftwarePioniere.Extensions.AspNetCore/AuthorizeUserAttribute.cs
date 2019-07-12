using Microsoft.AspNetCore.Authorization;

namespace SoftwarePioniere.Extensions.AspNetCore
{
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        public AuthorizeUserAttribute() : base(PolicyConstants.IsUserPolicy)
        {

        }
    }
}