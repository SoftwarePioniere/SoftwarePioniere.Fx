using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SoftwarePioniere.Extensions.AspNetCore.Auth0
{
    public static class Auth0AuthenticationBuilderExtensions
    {

        public static AuthenticationBuilder AddAuth0Bearer(this AuthenticationBuilder builder, Action<string> log)
        {
            log("AddAuth0Bearer");

            builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureAuth0JwtBearerOptions>();
            builder.AddJwtBearer();

            return builder;
        }

        private class ConfigureAuth0JwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
        {
            private readonly Auth0Options _auth0Options;

            public ConfigureAuth0JwtBearerOptions(IOptions<Auth0Options> auth0Options)
            {
                _auth0Options = auth0Options.Value;
            }

            public void Configure(JwtBearerOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            public void Configure(string name, JwtBearerOptions options)
            {
                string[] contextTokenAddPaths = null;
                if (!string.IsNullOrEmpty(_auth0Options.ContextTokenAddPaths)) contextTokenAddPaths = _auth0Options.ContextTokenAddPaths.Split(';');


                options.Audience = _auth0Options.Audience;
                options.Authority = _auth0Options.Domain;

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.SecurityToken is JwtSecurityToken token)
                            if (context.Principal.Identity is ClaimsIdentity identity)
                            {
                                identity.AddClaim(new Claim("access_token", token.RawData));
                                identity.AddClaim(new Claim("tenant", _auth0Options.TenantId));
                                identity.AddClaim(new Claim("provider", "auth0"));
                            }

                        return Task.CompletedTask;
                    },

                    OnMessageReceived = context =>
                    {
                        //https://docs.microsoft.com/de-de/aspnet/core/signalr/authn-and-authz?view=aspnetcore-2.1
                        if (contextTokenAddPaths != null)
                        {
                            var accessToken = context.Request.Query["access_token"];

                            var matchesAny = false;

                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                // If the request is for our hub...
                                var path = context.HttpContext.Request.Path;

                                foreach (var s in contextTokenAddPaths)
                                    if (path.StartsWithSegments(s))
                                    {
                                        matchesAny = true;
                                        break;
                                    }
                            }

                            if (matchesAny)
                                // Read the token out of the query string
                                context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            }
        }


    }
}