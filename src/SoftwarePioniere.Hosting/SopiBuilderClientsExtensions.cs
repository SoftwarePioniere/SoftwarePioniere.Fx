using System;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Builder;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderClientsExtensions
    {
        public static ISopiBuilder AddClients(this ISopiBuilder builder)
        {
            var config = builder.Config;

            builder.Log("Adding Auth0 Config");
            builder.Services.AddAuth0ClientOptions(config);

            builder.Log("Adding AzureAd Config");
            builder.Services.AddAzureAdClientOptions(config);

            builder.Log($"Auth Config Value: {builder.Options.Auth}");
            switch (builder.Options.Auth)
            {
                case SopiOptions.AuthAuth0:
                    builder.Log("Adding Auth0 Token Provider");
                    builder.Services.AddAuth0TokenProvider();
                    break;
                case SopiOptions.AuthAzureAd:
                    builder.Log("Adding AzureAd Token Provider");
                    builder.Services.AddAzureAdTokenProvider();
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Auth: {builder.Options.Auth}");

            }

            return builder;
        }
    }
}
