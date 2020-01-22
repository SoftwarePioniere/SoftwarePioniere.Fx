using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.Extensions.AspNetCore.AzureAd
{
    public static class AzureAdConfig
    {
        public static IServiceCollection ConfigureAzureAd(this IServiceCollection services, IConfiguration config, Action<string> log)
        {
            services.AddAzureAdOptions(config, log);

            services
                .AddAuthentication(AuthConfig.AuthenticationConfig)
                .AddAzureAdBearer(log)
                ;

            services.AddAzureAdAuthorization(log);

            return services;
        }
    }
}
