using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace SoftwarePioniere.Extensions.Hosting
{
    public static class ApplicationBuilderHealthChecksExtensions
    {
        public static IApplicationBuilder UseSopiHealthChecks(this IApplicationBuilder app)
        {
            app
                //.UseHealthChecksUI()
                .UseHealthChecks("/healthz",
                    new HealthCheckOptions()
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    })
                .UseHealthChecks("/health");
            return app;
        }
    }
}
