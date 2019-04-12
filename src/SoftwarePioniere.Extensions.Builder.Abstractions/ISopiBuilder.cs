using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.Extensions.Builder
{
    public interface ISopiBuilder
    {
        IServiceCollection Services { get; }

        object MvcBuilder { get; set; }

        object HealthChecksBuilder { get; set; }
        
        SopiOptions Options { get; set; }

        IConfiguration Config { get; set; }

        string Version { get;  }
    }
}