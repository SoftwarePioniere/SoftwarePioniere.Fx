
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.AspNetCore.Auth0
{
    public static class Auth0Config
    {
        public static ISwaggerClientOptions ConfigureAuth0(IConfiguration config, IServiceCollection services)
        {
            services.AddAuth0Options(config);

            services.AddAuthentication(AuthConfig.AuthenticationConfig)
                .AddAuth0(options => config.Bind("Auth0", options))
                //  .AddAzureAd(options => context.Configuration.Bind("AzureAd", options))
                ;

            var authOptions = new Auth0Options();
            config.Bind("Auth0", authOptions);

            return authOptions;
        }
    }
}
