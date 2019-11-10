using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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

        private static string ValidateIssuerWithPlaceholder(string issuer, SecurityToken token, TokenValidationParameters parameters)
        {
            //https://thomaslevesque.com/2018/12/24/multitenant-azure-ad-issuer-validation-in-asp-net-core/
            // Accepts any issuer of the form "https://login.microsoftonline.com/{tenantid}/v2.0",
            // where tenantid is the tid from the token.

            if (token is JwtSecurityToken jwt)
            {
                if (jwt.Payload.TryGetValue("tid", out var value) &&
                    value is string tokenTenantId)
                {
                    var validIssuers = (parameters.ValidIssuers ?? Enumerable.Empty<string>())
                        .Append(parameters.ValidIssuer)
                        .Where(i => !string.IsNullOrEmpty(i));

                    if (validIssuers.Any(i => i.Replace("{tenantid}", tokenTenantId) == issuer))
                        return issuer;
                }
            }

            {
                // Recreate the exception that is thrown by default
                // when issuer validation fails
                var validIssuer = parameters.ValidIssuer ?? "null";
                var validIssuers = parameters.ValidIssuers == null
                    ? "null"
                    : !parameters.ValidIssuers.Any()
                        ? "empty"
                        : string.Join(", ", parameters.ValidIssuers);
                string errorMessage = FormattableString.Invariant(
                    $"IDX10205: Issuer validation failed. Issuer: '{issuer}'. Did not match: validationParameters.ValidIssuer: '{validIssuer}' or validationParameters.ValidIssuers: '{validIssuers}'.");

                throw new SecurityTokenInvalidIssuerException(errorMessage)
                {
                    InvalidIssuer = issuer
                };

            }
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
                    IssuerValidator = ValidateIssuerWithPlaceholder,
                    //ValidIssuer = $"https://sts.windows.net/{_azureAdOptions.TenantId}/",
                    ValidateAudience = true,
                    ValidAudience = _azureAdOptions.Resource,
                    NameClaimType = _azureAdOptions.NameClaimType
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
                options.Authority = $"https://login.microsoftonline.com/{_azureAdOptions.TenantId}/";
                options.ClaimsIssuer = $"https://sts.windows.net/{_azureAdOptions.TenantId}/";
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


