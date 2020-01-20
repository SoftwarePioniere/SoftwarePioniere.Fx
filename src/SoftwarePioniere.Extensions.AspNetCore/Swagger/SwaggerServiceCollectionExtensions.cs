using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SoftwarePioniere.Extensions.AspNetCore.Swagger
{
    public static class SopiSwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddSopiSwaggerForMultipleServices(this IServiceCollection services, string titleName, string baseRoute, string serviceName, string[] apiKeys, bool readOnly
            , Action<SwaggerGenOptions> configureSwaggerGenOptions
            , Action<SopiSwaggerOptions> configureOptions = null)
        {
            Console.WriteLine("AddSopiSwaggerForMultipleServices");

            services.AddSingleton<IConfigureOptions<SopiSwaggerOptions>>(p =>
                new ConfigureSopiSwaggerOptionsForMultipleServices(
                    p.GetRequiredService<IOptions<SopiSwaggerAuthOptions>>(),
                    titleName, baseRoute, serviceName, apiKeys, readOnly, configureOptions
                    ));

            services
                .AddSopiSwagger(configureSwaggerGenOptions: configureSwaggerGenOptions)
                .AddSwaggerGen();

            return services;
        }

        private class ConfigureSopiSwaggerOptionsForMultipleServices : IConfigureOptions<SopiSwaggerOptions>
        {
            private readonly string _titleName;
            private readonly string _baseRoute;
            private readonly string _serviceName;
            private readonly string[] _apiKeys;
            private readonly bool _readOnly;
            private readonly Action<SopiSwaggerOptions> _configureOptions;
            private readonly SopiSwaggerAuthOptions _auth;

            public ConfigureSopiSwaggerOptionsForMultipleServices(
                IOptions<SopiSwaggerAuthOptions> swaggerClientOptions,
                string titleName, string baseRoute, string serviceName, string[] apiKeys, bool readOnly
                , Action<SopiSwaggerOptions> configureOptions = null)
            {
                _auth = swaggerClientOptions.Value;
                _titleName = titleName;
                _baseRoute = baseRoute;
                _serviceName = serviceName;
                _apiKeys = apiKeys;
                _readOnly = readOnly;
                _configureOptions = configureOptions;
            }
            public void Configure(SopiSwaggerOptions options)
            {
                options.Title = $"{_titleName} {_serviceName}";
                options.Doc = _apiKeys[0];

                var docs = _apiKeys.Select(apiKey => new SwaggerDocOptions { Name = apiKey, Title = options.Title, Url = $"/{_baseRoute}/{_serviceName}/swagger/{apiKey}.json" }).ToArray();
                options.Docs = docs;
                options.RouteTemplate = $"{_baseRoute}/{_serviceName}" + "/swagger/{documentName}.json";
                options.ReadOnlyUi = _readOnly;

                if (!_readOnly)
                {
                    options.Scopes = new Dictionary<string, string>();

                    options.OAuth2Scheme = new OpenApiSecurityScheme()
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows()
                        {
                            Implicit = new OpenApiOAuthFlow()
                            {
                                Scopes = options.Scopes,
                                AuthorizationUrl = _auth.AuthorizationUrl
                            }
                        }
                    };

                    options.OAuthAdditionalQueryStringParams = new Dictionary<string, string>
                    {
                        {"resource", _auth.Resource},
                        {"Audience", _auth.Resource}
                    };

                    options.OAuthClientId = _auth.ClientId;
                    options.OAuthClientSecret = _auth.ClientSecret;

                }
                _configureOptions?.Invoke(options);
            }
        }

        //public static IServiceCollection AddSopiSwaggerForSingleService(this IServiceCollection services, string apiKey, string baseRoute, string serviceName, bool readOnly
        //, Action<SopiSwaggerOptions> configureOptions = null)
        //{
        //    Console.WriteLine("AddSopiSwaggerForSingleService");


        //    services.AddSingleton<IConfigureOptions<SopiSwaggerOptions>>(p =>
        //        new ConfigureSopiSwaggerForSingleService(
        //            p.GetRequiredService<IOptions<SopiSwaggerClientOptions>>(),
        //            apiKey, baseRoute, serviceName, readOnly, configureOptions
        //        ));

        //    return services;
        //}

        //private class ConfigureSopiSwaggerForSingleService : IConfigureOptions<SopiSwaggerOptions>
        //{
        //    private readonly string _apiKey;
        //    private readonly string _baseRoute;
        //    private readonly string _serviceName;
        //    private readonly bool _readOnly;
        //    private readonly Action<SopiSwaggerOptions> _configureOptions;
        //    private readonly SopiSwaggerClientOptions _swaggerClientOptions;

        //    public ConfigureSopiSwaggerForSingleService(
        //        IOptions<SopiSwaggerClientOptions> swaggerClientOptions,
        //        string apiKey, string baseRoute, string serviceName, bool readOnly
        //        , Action<SopiSwaggerOptions> configureOptions = null)
        //    {
        //        _swaggerClientOptions = swaggerClientOptions.Value;

        //        _apiKey = apiKey;
        //        _baseRoute = baseRoute;
        //        _serviceName = serviceName;
        //        _readOnly = readOnly;
        //        _configureOptions = configureOptions;
        //    }
        //    public void Configure(SopiSwaggerOptions c)
        //    {
        //        c.Title = $"{_apiKey} {_serviceName}";
        //        c.Doc = _apiKey;
        //        c.Docs = new[]
        //        {
        //            new SwaggerDocOptions{Name = _apiKey , Title  = c.Title,  Url=$"/{_baseRoute}/{_serviceName}/swagger/{_apiKey}.json" }
        //        };
        //        c.RouteTemplate = $"{_baseRoute}/{_serviceName}" + "/swagger/{documentName}.json";
        //        c.ServiceName = _serviceName;
        //        c.ReadOnlyUi = _readOnly;

        //        if (!_readOnly)
        //        {

        //            var scopes = new Dictionary<string, string>();
        //            //{
        //            //    {"admin", "admin access"}
        //            //};

        //            //c.OAuth2Scheme = new OAuth2Scheme
        //            //{
        //            //    Type = "oauth2",
        //            //    Flow = "implicit",
        //            //    AuthorizationUrl = _swaggerClientOptions.AuthorizationUrl,
        //            //    Scopes = scopes
        //            //};

        //            c.OAuthAdditionalQueryStringParams = new Dictionary<string, string>
        //            {
        //                {"resource", _swaggerClientOptions.Resource},
        //                {"Audience", _swaggerClientOptions.Resource}
        //            };

        //            c.OAuthClientId = _swaggerClientOptions.ClientId;
        //            c.OAuthClientSecret = _swaggerClientOptions.ClientSecret;
        //        }
        //        _configureOptions?.Invoke(c);
        //    }


        //}

        public static IServiceCollection AddSopiSwagger(this IServiceCollection services
            , Action<SopiSwaggerOptions> configureSopiSwaggerOptions = null
            , Action<SwaggerGenOptions> configureSwaggerGenOptions = null)
        {
            Console.WriteLine("AddSopiSwagger");

            if (configureSopiSwaggerOptions != null)
            {
                services.AddOptions()
                    .Configure(configureSopiSwaggerOptions);
            }

            //  services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGen>();

            services.AddOptions<SwaggerGenOptions>()
                .Configure<IOptions<SopiSwaggerOptions>>((options, sopiSwaggerOptionsT) =>
                {
                    var sopiSwaggerOptions = sopiSwaggerOptionsT.Value;
                    if (sopiSwaggerOptions.Docs != null)
                    {
                        foreach (var doc in sopiSwaggerOptions.Docs)
                        {
                            options.SwaggerDoc(doc.Name, new OpenApiInfo
                            {
                                Title = doc.Title,
                                Version = "v1"
                            });
                        }
                    }
                    else
                    {
                        var doc = "api";

                        if (!string.IsNullOrEmpty(sopiSwaggerOptions.Doc))
                        {
                            doc = sopiSwaggerOptions.Doc;
                        }

                        options.SwaggerDoc(doc, new OpenApiInfo
                        {
                            Title = $"{sopiSwaggerOptions.Title}-{doc}",
                            Version = "v1"
                        });
                    }

                    if (sopiSwaggerOptions.XmlFiles != null)
                    {
                        foreach (var xmlFile in sopiSwaggerOptions.XmlFiles)
                        {
                            IncludeXmlCommentsIfExist(options, xmlFile);
                        }
                    }

                    options.EnableAnnotations();
                    //     options.DescribeAllEnumsAsStrings();
                    options.OperationFilter<SummaryFromOperationFilter>();
                    //options.OperationFilter<FormFileOperationFilter>();
                    options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();


                    if (!sopiSwaggerOptions.ReadOnlyUi)
                    {
                        if (sopiSwaggerOptions.OAuth2Scheme != null)
                        {
                            options.AddSecurityDefinition("oauth2", sopiSwaggerOptions.OAuth2Scheme);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(sopiSwaggerOptions.AuthorizationUrl))
                            {
                                var sch = new OpenApiSecurityScheme
                                {
                                    Type = SecuritySchemeType.OAuth2,
                                    Flows = new OpenApiOAuthFlows
                                    {
                                        Implicit = new OpenApiOAuthFlow
                                        {
                                            AuthorizationUrl = new Uri(sopiSwaggerOptions.AuthorizationUrl),
                                            Scopes = sopiSwaggerOptions.Scopes
                                        }
                                    }
                                };
                                options.AddSecurityDefinition("oauth2", sch);
                            }
                        }
                    }

                    options.OperationFilter<SecurityRequirementsOperationFilter>();

                    options.DocInclusionPredicate((s, description) =>
                    {
                        if (sopiSwaggerOptions.Docs == null)
                        {
                            return true;
                        }

                        if (sopiSwaggerOptions.Docs.Select(x => x.Name).Contains(s))
                        {
                            return description.GroupName == s;
                        }

                        if (string.IsNullOrEmpty(description.GroupName))
                        {
                            return true;
                        }

                        return description.GroupName != s;
                    });

                    configureSwaggerGenOptions?.Invoke(options);

                });

            services.AddSwaggerGen();

            return services;
        }

        private static void IncludeXmlCommentsIfExist(this SwaggerGenOptions swaggerGenOptions, string fileName)
        {
            //var xmlFileName = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, fileName);
            var xmlFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            //var xmlFileName = fileName;
            var exist = File.Exists(xmlFileName);

            if (exist)
            {
                swaggerGenOptions.IncludeXmlComments(xmlFileName);
            }
        }

        //public class ConfigureSwaggerGen : IConfigureNamedOptions<SwaggerGenOptions>
        //{
        //    private readonly SopiSwaggerOptions _sopiSwaggerOptions;

        //    public ConfigureSwaggerGen(IOptions<SopiSwaggerOptions> sopiSwaggerOptions)
        //    {
        //        _sopiSwaggerOptions = sopiSwaggerOptions.Value;
        //    }

        //    public void Configure(SwaggerGenOptions options)
        //    {
        //        Configure(Options.DefaultName, options);
        //    }

        //    public void Configure(string name, SwaggerGenOptions options)
        //    {
        //        if (_sopiSwaggerOptions.Docs != null)
        //        {
        //            foreach (var doc in _sopiSwaggerOptions.Docs)
        //            {
        //                options.SwaggerDoc(doc.Name, new OpenApiInfo
        //                {
        //                    Title = doc.Title,
        //                    Version = "v1"
        //                });
        //            }
        //        }
        //        else
        //        {
        //            var doc = "api";

        //            if (!string.IsNullOrEmpty(_sopiSwaggerOptions.Doc))
        //            {
        //                doc = _sopiSwaggerOptions.Doc;
        //            }

        //            options.SwaggerDoc(doc, new OpenApiInfo
        //            {
        //                Title = $"{_sopiSwaggerOptions.Title}-{doc}",
        //                Version = "v1"
        //            });
        //        }

        //        if (_sopiSwaggerOptions.XmlFiles != null)
        //        {
        //            foreach (var xmlFile in _sopiSwaggerOptions.XmlFiles)
        //            {
        //                IncludeXmlCommentsIfExist(options, xmlFile);
        //            }
        //        }

        //        options.EnableAnnotations();
        //        //options.DescribeAllEnumsAsStrings();
        //        //options.OperationFilter<FormFileOperationFilter>();
        //        options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
        //        options.OperationFilter<SummaryFromOperationFilter>();

        //        if (!string.IsNullOrEmpty(_sopiSwaggerOptions.AuthorizationUrl))
        //        {
        //            var sch = new OpenApiSecurityScheme
        //            {
        //                Name = "oauth2",
        //                Flows = new OpenApiOAuthFlows
        //                {
        //                    Implicit = new OpenApiOAuthFlow
        //                    {
        //                        AuthorizationUrl = new Uri(_sopiSwaggerOptions.AuthorizationUrl),
        //                        Scopes = _sopiSwaggerOptions.Scopes
        //                    }
        //                }
        //            };
        //            options.AddSecurityDefinition("oauth2", sch);

        //            //var sch = new OAuth2Scheme
        //            //{
        //            //    Type = "oauth2",
        //            //    Flow = "implicit",
        //            //    AuthorizationUrl = _sopiSwaggerOptions.AuthorizationUrl,
        //            //    Scopes = _sopiSwaggerOptions.Scopes
        //            //};
        //            //options.AddSecurityDefinition("oauth2", sch);
        //        }

        //        options.AddSecurityDefinition("oauth2", _sopiSwaggerOptions.OAuth2Scheme);

        //        options.OperationFilter<SecurityRequirementsOperationFilter>();

        //        options.DocInclusionPredicate((s, description) =>
        //        {
        //            if (_sopiSwaggerOptions.Docs == null)
        //            {
        //                return true;
        //            }

        //            if (_sopiSwaggerOptions.Docs.Select(x => x.Name).Contains(s))
        //            {
        //                return description.GroupName == s;
        //            }

        //            if (string.IsNullOrEmpty(description.GroupName))
        //            {
        //                return true;
        //            }

        //            return description.GroupName != s;
        //        });
        //    }
        //}
    }
}