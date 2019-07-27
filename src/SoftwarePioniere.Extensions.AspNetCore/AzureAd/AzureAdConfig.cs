using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.Extensions.AspNetCore.AzureAd
{
    public static class AzureAdConfig
    {
        public static IServiceCollection ConfigureAzureAd(this IServiceCollection services, IConfiguration config)
        {
            services.AddAzureAdOptions(config);

            services
                .AddAuthentication(AuthConfig.AuthenticationConfig)
                .AddAzureAdBearer()
                ;

            services.AddAzureAdAuthorization();

            return services;
        }
    }
}
