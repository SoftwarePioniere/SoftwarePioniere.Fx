using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.Builder
{
    public interface ISopiBuilder
    {
        IServiceCollection Services { get; }

        Dictionary<string, object> Features { get; }

        SopiOptions Options { get; set; }

        IConfiguration Config { get; set; }

        //string Version { get; }

        void Log(string message);
    }
}