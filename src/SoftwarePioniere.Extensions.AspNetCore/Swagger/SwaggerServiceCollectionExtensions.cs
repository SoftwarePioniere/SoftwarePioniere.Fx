using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SoftwarePioniere.Extensions.AspNetCore.Swagger
{
    public static class SwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddMySwagger(this IServiceCollection services, Action<MySwaggerOptions> configureOptions)
        {
            var options = new MySwaggerOptions();
            configureOptions(options);

            services
                .Configure(configureOptions)
                .AddSwaggerGen(c =>
            {
                if (options.Docs != null)
                {
                    foreach (var doc in options.Docs)
                    {
                        c.SwaggerDoc(doc.Name, new Info
                        {
                            Title = doc.Title,
                            Version = "v1"
                        });
                    }
                }
                else
                {
                    var doc = "api";

                    if (!string.IsNullOrEmpty(options.Doc))
                    {
                        doc = options.Doc;
                    }

                    c.SwaggerDoc(doc, new Info
                    {
                        Title = $"{options.Title}-{doc}",
                        Version = "v1"
                    });
                }

                if (options.XmlFiles != null)
                {
                    foreach (var xmlFile in options.XmlFiles)
                    {
                        IncludeXmlCommentsIfExist(c, xmlFile);
                    }
                }

                c.EnableAnnotations();
                c.DescribeAllEnumsAsStrings();
                c.OperationFilter<FormFileOperationFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                c.OperationFilter<SummaryFromOperationFilter>();

                if (!string.IsNullOrEmpty(options.AuthorizationUrl))
                {
                    var sch = new OAuth2Scheme
                    {
                        Type = "oauth2",
                        Flow = "implicit",
                        AuthorizationUrl = options.AuthorizationUrl,
                        Scopes = options.Scopes
                    };
                    c.AddSecurityDefinition("oauth2", sch);
                }

                c.AddSecurityDefinition("oauth2", options.OAuth2Scheme);

                c.OperationFilter<SecurityRequirementsOperationFilter>();

                c.DocInclusionPredicate((s, description) =>
                {
                    if (options.Docs == null) return true;

                    if (options.Docs.Select(x => x.Name).Contains(s))
                        return description.GroupName == s;

                    if (string.IsNullOrEmpty(description.GroupName))
                        return true;

                    return description.GroupName != s;
                });
            });

            return services;
        }

        public static void IncludeXmlCommentsIfExist(this SwaggerGenOptions swaggerGenOptions, string fileName)
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
    }
}