using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.Extensions.AspNetCore.Auth0
{
    public static class Auth0Config
    {
        public static IServiceCollection ConfigureAuth0(this IServiceCollection services, IConfiguration config, Action<string> log)
        {
            services.AddAuth0Options(config);

            services.AddAuthentication(AuthConfig.AuthenticationConfig)
                .AddAuth0Bearer(log)
                ;

            services.AddAuth0Authorization(log)
                ;

            return services;
        }
    }
}
