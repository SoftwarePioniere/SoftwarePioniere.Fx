using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Builder;

namespace SoftwarePioniere.AspNetCore.Builder
{
    public static class SopiBuilderMvcExtensions
    {
        public static ISopiBuilder AddMvcServices(this ISopiBuilder builder, Action<IMvcBuilder> mvcBuilderAction = null)
        {
            var services = builder.Services;

            var mvc = services.AddMvc()
                //.AddJsonOptions(options =>
                //{
                //    //      options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                //})
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                //.AddNewtonsoftJson()
                
                ;

            mvcBuilderAction?.Invoke(mvc);

            builder.AddFeature("MvcBuilder", mvc);

            services.AddCors(o => o.AddDefaultPolicy(pb =>
            {
                if (builder.Options.AllowedOrigins == "*")
                {
                    pb.AllowAnyOrigin();
                }
                else
                {
                    pb.WithOrigins(builder.Options.AllowedOrigins.Split(';'));
                    pb.AllowCredentials();
                }
                pb.AllowAnyHeader();
                pb.AllowAnyMethod();
            }));


            return builder;
        }

        public static IMvcBuilder GetMvcBuilder(this ISopiBuilder builder)
        {
            return builder.GetFeature<IMvcBuilder>("MvcBuilder");
        }

    }
}
