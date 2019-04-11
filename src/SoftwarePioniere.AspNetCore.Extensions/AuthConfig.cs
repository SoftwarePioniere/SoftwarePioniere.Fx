using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SoftwarePioniere.AspNetCore
{
    public static class AuthConfig
    {
        public static void AuthenticationConfig(AuthenticationOptions config)
        {
            config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    }
}
