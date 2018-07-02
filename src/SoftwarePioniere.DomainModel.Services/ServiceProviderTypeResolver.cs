using System;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.DomainModel.Services
{
    public class ServiceProviderTypeResolver : IResolveType
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;


        public ServiceProviderTypeResolver(IServiceProvider serviceProviderProvider, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProviderProvider ?? throw new ArgumentNullException(nameof(serviceProviderProvider));
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public object Resolve(Type t)
        {
            _logger.LogDebug("Resolving Type: {0}", t.Name);
            var o = _serviceProvider.GetService(t);

            if (o != null) return o;

            _logger.LogError("Cannot find Type {Type}. Please Register first ", t);
            throw new InvalidOperationException($"Cannot find Type {t.Name}. Please Register first");
        }
    }
}
