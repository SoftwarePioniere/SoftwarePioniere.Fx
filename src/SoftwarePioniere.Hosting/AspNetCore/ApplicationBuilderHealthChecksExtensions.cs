using Microsoft.AspNetCore.Builder;

namespace SoftwarePioniere.Hosting.AspNetCore
{
    public static class ApplicationBuilderHealthChecksExtensions
    {
        public static IApplicationBuilder UseSopiHealthChecks(this IApplicationBuilder app)
        {
            app
                //.UseHealthChecksUI()
                //.UseHealthChecks("/healthz",
                //    new HealthCheckOptions()
                //    {
                //        Predicate = _ => true,
                //        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                //    })
                .UseHealthChecks("/health");
            return app;
        }
    }
}
