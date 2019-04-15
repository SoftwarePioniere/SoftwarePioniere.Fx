using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.AspNetCore;

namespace SoftwarePioniere.Extensions.Builder
{
    public static class SopiBuilderMvcExtensions
    {
        public static ISopiBuilder AddMvcServices(this ISopiBuilder builder)
        {
            var services = builder.Services;

            var mvc = services.AddMvc(MvcConfig.DefaultConfig)
                .AddJsonOptions(options =>
                {
                    //      options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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
