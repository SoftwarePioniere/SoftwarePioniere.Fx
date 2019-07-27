using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Extensions.AspNetCore;
// ReSharper disable once RedundantUsingDirective
using SoftwarePioniere.Extensions.AspNetCore.Auth0;
// ReSharper disable once RedundantUsingDirective
using SoftwarePioniere.Extensions.AspNetCore.AzureAd;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;

namespace AuthSample
{
    public static class Program
    {
        //public static void MySwaggerConfig(Action<MySwaggerOptions> c)
        //{

        //}

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }


        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {


                    //   services.ConfigureAzureAd(context.Configuration);
                    services.ConfigureAuth0(context.Configuration);
                    services.AddSingleton<IConfigureOptions<SopiSwaggerOptions>, SwaggerConfig>();
                    
                    services.AddMvc(MvcConfig.DefaultConfig)
                        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

                    services.AddSopiSwagger();

                })

                .Configure(app =>
                {
                    var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

                    app.When(env.IsDevelopment(), app.UseDeveloperExceptionPage)
                        .When(!env.IsDevelopment(), app.UseHsts)
                        .UseAuthentication()
                        .UseMvc()
                        .UseSopiSwagger()

                        ;
                })
            ;




    }
}
