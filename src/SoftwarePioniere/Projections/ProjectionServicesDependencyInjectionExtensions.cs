using System;
using SoftwarePioniere.Projections;


// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddProjectionServices(this IServiceCollection services, Action<ProjectionOptions> configure = null)
        {
            services.AddSingleton<IProjectorServices, ProjectorServices>();

            if (configure != null)
            {
                services.AddOptions()
                    .Configure(configure);

            }
            else
            {
                services
                    .AddOptions<ProjectionOptions>()
                    ;
            }

            return services;
        }


    }
}
