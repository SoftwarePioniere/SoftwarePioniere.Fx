using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.Extensions.Builder
{
    public class SopiBuilder : ISopiBuilder
    {
        public IServiceCollection Services { get; }
        public IDictionary<string, object> Features { get; } = new Dictionary<string, object>();
        //public object MvcBuilder { get; set; }
        public SopiOptions Options { get; set; }
        public IConfiguration Config { get; set; }
        public string Version { get; }
        //public object HealthChecksBuilder { get; set; }

        public SopiBuilder(IServiceCollection services)
        {
            //      AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Services = services ?? throw new ArgumentNullException(nameof(services));
            var assembly = Assembly.GetEntryAssembly();
            Version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        //private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    //Console.WriteLine($"Fliegel365Builder: Assembly Load Failed: {args.Name}");

        //    var names = args.Name.Split(',');
        //    if (names.Length > 0)
        //    {
        //        var name = $"{names[0].Trim()}"; //, {names[1].Trim()}";

        //        if (name.StartsWith("Fliegel365") ||
        //            name.StartsWith("SoftwarePioniere"))
        //        {
        //            Console.WriteLine($"Fliegel365Builder: Try Loading Assembly {name}");
        //            var ass = Assembly.Load(name);
        //            return ass;
        //        }

        //    }

        //    //Console.WriteLine("Fliegel365Builder: Cannot load Assembly");
        //    return null;
        //}
    }
}
