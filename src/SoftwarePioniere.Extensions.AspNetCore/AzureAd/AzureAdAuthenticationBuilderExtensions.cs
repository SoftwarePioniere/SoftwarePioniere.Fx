using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SoftwarePioniere.Extensions.AspNetCore.AzureAd
{
    public static class AzureAdAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddAzureAdBearer(this AuthenticationBuilder builder)
        {
            Console.WriteLine("AddAzureAdBearer");

            builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureAzureJwtBearerOptions>();
            builder.AddJwtBearer();

            return builder;

        }

        private class ConfigureAzureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
        {
            private readonly AzureAdOptions _azureAdOptions;

            public ConfigureAzureJwtBearerOptions(IOptions<AzureAdOptions> azureOptions)
            {
                _azureAdOptions = azureOptions.Value;
            }

            public void Configure(string name, JwtBearerOptions options)
            {
                
                var tokenValParam = new TokenValidationParameters()
                {
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidIssuer = $"https://sts.windows.net/{_azureAdOptions.TenantId}/",
                    ValidateAudience = true,
                    ValidAudience = _azureAdOptions.Resource
                };

                if (!string.IsNullOrEmpty(_azureAdOptions.IssuerSigningKey) && !string.Equals(_azureAdOptions.IssuerSigningKey, "XXX", StringComparison.InvariantCultureIgnoreCase))
                {
                    tokenValParam.ValidateIssuerSigningKey = true;
                    tokenValParam.IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_azureAdOptions.IssuerSigningKey));
                }

                string[] contextTokenAddPaths = null;
                if (!string.IsNullOrEmpty(_azureAdOptions.ContextTokenAddPaths))
                {
                    contextTokenAddPaths = _azureAdOptions.ContextTokenAddPaths.Split(';');
                }

                Console.WriteLine("AddAzureAd: Adding JwtBeaerer");


                options.Audience = _azureAdOptions.Resource;
                options.Authority =  $"https://login.microsoftonline.com/{_azureAdOptions.TenantId}/" ;
                options.ClaimsIssuer =$"https://sts.windows.net/{_azureAdOptions.TenantId}/";
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValParam;

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.SecurityToken is JwtSecurityToken token)
                        {
                            if (context.Principal.Identity is ClaimsIdentity identity)
                            {
                                identity.AddClaim(new Claim("access_token", token.RawData));
                                identity.AddClaim(new Claim("tenant", _azureAdOptions.TenantId));
                                identity.AddClaim(new Claim("provider", "aad"));
                            }
                        }

                        return Task.CompletedTask;
                    },

                    OnMessageReceived = context =>
                    {
                        //https://docs.microsoft.com/de-de/aspnet/core/signalr/authn-and-authz?view=aspnetcore-2.1
                        if (contextTokenAddPaths != null)
                        {
                            var accessToken = context.Request.Query["access_token"];

                            bool matchesAny = false;

                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                // If the request is for our hub...
                                var path = context.HttpContext.Request.Path;

                                foreach (var s in contextTokenAddPaths)
                                {
                                    if (path.StartsWithSegments(s))
                                    {
                                        matchesAny = true;
                                        break;
                                    }
                                }
                            }

                            if (matchesAny)
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                        }

                        return Task.CompletedTask;
                    }
                };


            }

            public void Configure(JwtBearerOptions options)
            {
                Configure(Options.DefaultName, options);
            }
        }
    }
}


