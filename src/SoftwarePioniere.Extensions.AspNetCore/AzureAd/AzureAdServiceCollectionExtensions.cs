using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;

namespace SoftwarePioniere.Extensions.AspNetCore.AzureAd
{
    public static class AzureAdServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureAdAuthorization(this IServiceCollection services)
        {
            Console.WriteLine("AddAzureAdAuthorization");

            //configure the AuthorizationOptions from the AzureAd Options
            services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureAzureAuthorizationOptions>();

            //adds authtorization for registered options
            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddAzureAdOptions(this IServiceCollection services, IConfiguration config)
        {
            return AddAzureAdOptions(services, options => config.Bind("AzureAd", options));
        }

        public static IServiceCollection AddAzureAdOptions(this IServiceCollection services, Action<AzureAdOptions> configureOptions)
        {
            Console.WriteLine("AddAzureAdOptions");

            services.AddOptions()
                .Configure(configureOptions);

            services.AddSingleton<IConfigureOptions<SopiSwaggerAuthOptions>, ConfigureSopiSwaggerClientOptions>();
            return services;
        }

        private class ConfigureAzureAuthorizationOptions : IConfigureNamedOptions<AuthorizationOptions>
        {
            private readonly AzureAdOptions _azureAdOptions;

            public ConfigureAzureAuthorizationOptions(IOptions<AzureAdOptions> azureOptions)
            {
                _azureAdOptions = azureOptions.Value;
            }

            public void Configure(AuthorizationOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            public void Configure(string name, AuthorizationOptions options)
            {
                if (!string.IsNullOrEmpty(_azureAdOptions.AdminGroupId)) options.AddPolicy(PolicyConstants.IsAdminPolicy, policy => policy.RequireClaim("groups", _azureAdOptions.AdminGroupId));

                if (!string.IsNullOrEmpty(_azureAdOptions.UserGroupId)) options.AddPolicy(PolicyConstants.IsUserPolicy, policy => policy.RequireClaim("groups", _azureAdOptions.UserGroupId));
            }
        }

        public class ConfigureSopiSwaggerClientOptions : IConfigureNamedOptions<SopiSwaggerAuthOptions>
        {
            private readonly AzureAdOptions _azureAdOptions;

            public ConfigureSopiSwaggerClientOptions(IOptions<AzureAdOptions> auth0Options)
            {
                _azureAdOptions = auth0Options.Value;
            }

            public void Configure(SopiSwaggerAuthOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            public string Authority => $"https://login.microsoftonline.com/{_azureAdOptions.TenantId}/";
            public string SwaggerAuthorizationUrl => $"{Authority}oauth2/authorize";

            public void Configure(string name, SopiSwaggerAuthOptions options)

            {

                //public string Authority => $"https://login.microsoftonline.com/{TenantId}/";
                //public string IssuerUrl => $"https://sts.windows.net/{TenantId}/";
                //public string SwaggerAuthorizationUrl => $"{Authority}oauth2/authorize";
                //public string SwaggerResource => Resource;

                options.AuthorizationUrl = new Uri(SwaggerAuthorizationUrl);
                options.ClientId = _azureAdOptions.SwaggerClientId;
                options.ClientSecret = _azureAdOptions.SwaggerClientSecret;
                options.Resource = _azureAdOptions.Resource;
            }
        }
    }
}