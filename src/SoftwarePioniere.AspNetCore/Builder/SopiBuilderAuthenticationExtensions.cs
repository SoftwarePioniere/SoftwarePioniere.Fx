using System;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Extensions.AspNetCore.Auth0;
using SoftwarePioniere.Extensions.AspNetCore.AzureAd;

namespace SoftwarePioniere.AspNetCore.Builder
{
    public static class SopiBuilderAuthenticationExtensions
    {
        public static ISopiBuilder AddAuthentication(this ISopiBuilder builder)
        {
            builder.Log($"Sopi Auth Config Value: {builder.Options.Auth}");
            switch (builder.Options.Auth)
            {
                case SopiOptions.AuthAuth0:
                    {
                        builder.Log("Adding Auth0 Config");
                        builder.Services.ConfigureAuth0(builder.Config, builder.Log);
                        //builder.AddFeature("SwaggerClientOptions", swag);
                        break;
                    }
                case SopiOptions.AuthAzureAd:
                    {
                        builder.Log("Adding AzureAd Config");
                        builder.Services.ConfigureAzureAd(builder.Config, builder.Log);
                        //builder.AddFeature("SwaggerClientOptions", swag);
                        break;
                    }
                default:
                    throw new InvalidOperationException($"Invalid Auth: {builder.Options.Auth}");

            }

            return builder;

        }

    }
}
