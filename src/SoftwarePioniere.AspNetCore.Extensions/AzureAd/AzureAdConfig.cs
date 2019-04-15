using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.AspNetCore.AzureAd
{
    public static class AzureAdConfig
    {
        public static ISwaggerClientOptions ConfigureAzureAd(IConfiguration config, IServiceCollection services)
        {
            services.AddAzureAdOptions(config);

            services.AddAuthentication(AuthConfig.AuthenticationConfig)
                .AddAzureAd(options => config.Bind("AzureAd", options))
                ;

            var authOptions = new AzureAdOptions();
            config.Bind("AzureAd", authOptions);

            services.AddSingleton<ISwaggerClientOptions>(authOptions);

            return authOptions;
        }
    }
}
