using System;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Extensions.AspNetCore;
using SoftwarePioniere.Extensions.AspNetCore.Auth0;
using SoftwarePioniere.Extensions.AspNetCore.AzureAd;

namespace SoftwarePioniere.AspNetCore.Builder
{
    public static class SopiBuilderAuthenticationExtensions
    {
        public static ISopiBuilder AddAuthentication(this ISopiBuilder builder)
        {

            Console.WriteLine($"Sopi Auth Config Value: {builder.Options.Auth}");
            switch (builder.Options.Auth)
            {
                case SopiOptions.AuthAuth0:
                    {
                        Console.WriteLine("Adding Auth0 Config");
                        var swag = Auth0Config.ConfigureAuth0(builder.Config, builder.Services);
                        builder.AddFeature("SwaggerClientOptions", swag);
                        break;
                    }
                case SopiOptions.AuthAzureAd:
                    {
                        Console.WriteLine("Adding AzureAd Config");
                        var swag = AzureAdConfig.ConfigureAzureAd(builder.Config, builder.Services);
                        builder.AddFeature("SwaggerClientOptions", swag);
                        break;
                    }
                default:
                    throw new InvalidOperationException($"Invalid Auth: {builder.Options.Auth}");

            }

            return builder;

        }

        public static ISwaggerClientOptions GetSwaggerClientOptions(this ISopiBuilder builder)
        {
            return builder.GetFeature<ISwaggerClientOptions>("SwaggerClientOptions");
        }

        //public static ISopiBuilder AddAuth0Authentication(this ISopiBuilder f365,
        //    Action<Auth0Options> configureOptions)
        //{
        //    var services = f365.Services;

        //    services.AddAuthentication(options =>
        //        {
        //            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //        })
        //        .AddAuth0(configureOptions);

        //    services.Configure(configureOptions);

        //    f365.Services
        //    //f365.Services.PostConfigure<Auth0Options>(c =>
        //    //{
        //    //    Console.WriteLine(c.SwaggerClientId);
        //    //});


        //    return f365;
        //}

        //public static ISopiBuilder AddAzureAdAuthentication(this ISopiBuilder f365,
        //    Action<AzureAdOptions> configureOptions)
        //{
        //    var services = f365.Services;

        //    services.AddAuthentication(options =>
        //        {
        //            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //        })
        //        .AddAzureAd(configureOptions);

        //    services.Configure(configureOptions);

        //    f365.Services.PostConfigure<SopiOptions>(c =>
        //    {
        //        c.Auth = SopiOptions.AuthAzureAd;
        //    });

        //    //  f365.Services.PostConfigure<AzureAdOptions>(c => { c.ContextTokenAddPaths = f365.Options.WebSocketPaths; });

        //    return f365;
        //}
    }
}
