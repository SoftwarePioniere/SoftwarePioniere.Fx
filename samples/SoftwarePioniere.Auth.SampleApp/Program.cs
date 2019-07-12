using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SoftwarePioniere.AspNetCore;
// ReSharper disable once RedundantUsingDirective
using SoftwarePioniere.AspNetCore.Auth0;
// ReSharper disable once RedundantUsingDirective
using SoftwarePioniere.AspNetCore.AzureAd;
using SoftwarePioniere.AspNetCore.Swagger;

namespace SoftwarePioniere.Auth.SampleApp
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

                    services.AddMvc(MvcConfig.DefaultConfig)
                        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

                    //var authOptions = Auth0Config.ConfigureAuth0(context.Configuration, services);
                    var authOptions = AzureAdConfig.ConfigureAzureAd(context.Configuration, services);

                    services.AddMySwagger(c => SwaggerConfig.TestApi(c, authOptions));
                })

                .Configure(app =>
                {
                    var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

                    var authOptions = app.ApplicationServices.GetRequiredService<IOptions<AzureAdOptions>>().Value;

                    app.When(env.IsDevelopment(), app.UseDeveloperExceptionPage)
                        .When(!env.IsDevelopment(), app.UseHsts)
                        .UseAuthentication()
                        .UseMvc()
                        .UseMySwagger(c => SwaggerConfig.TestApi(c, authOptions))

                        ;
                })
            ;




    }
}
