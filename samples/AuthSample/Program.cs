// ReSharper disable once RedundantUsingDirective
// ReSharper disable once RedundantUsingDirective

using System;
using SoftwarePioniere.Extensions.AspNetCore.AzureAd;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Extensions.AspNetCore;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;

namespace AuthSample
{
    public static class Program
    {
        private static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices((context, services) =>
                    {
                        services.ConfigureAzureAd(context.Configuration, Console.WriteLine);
                        //services.ConfigureAuth0(context.Configuration);

                        services.AddSingleton<IConfigureOptions<SopiSwaggerOptions>, SwaggerConfig>();

                        services.AddControllers();

                        services.AddMvc()
                            .SetCompatibilityVersion(CompatibilityVersion.Latest)
                            .AddNewtonsoftJson();


                        services.AddSopiSwagger(Console.WriteLine);
                    });


                    webBuilder.Configure(app =>
                    {
                        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

                        app.When(env.IsDevelopment(), app.UseDeveloperExceptionPage)
                            .When(!env.IsDevelopment(), app.UseHsts)
                            .UseRouting()
                            .UseAuthentication()
                            .UseAuthorization()
                            .UseEndpoints(endpoints =>
                            {
                                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                            })
                            .UseSopiSwagger()
                            ;
                    });
                });
        }

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
    }
}