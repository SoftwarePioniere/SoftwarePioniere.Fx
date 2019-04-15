using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.Extensions.Builder
{
    public interface ISopiBuilder
    {
        IServiceCollection Services { get; }

        IDictionary<string, object> Features { get; }

        SopiOptions Options { get; set; }

        IConfiguration Config { get; set; }

        string Version { get; }
    }
}