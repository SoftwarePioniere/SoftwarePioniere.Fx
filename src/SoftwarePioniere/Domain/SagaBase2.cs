using System;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Lock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Domain
{
    public abstract class SagaBase2 : ISaga
    {
        protected ISagaServices Services { get; }

        protected readonly IMessageBusAdapter Bus;

        protected readonly ILogger Logger;

        protected SagaBase2(ILoggerFactory loggerFactory, ISagaServices services)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            Services = services;

            Logger = loggerFactory.CreateLogger(GetType());
            Bus = services.Bus;
            Repository = services.Repository;
            LockProvider = services.LockProvider;
        }

        protected CancellationToken CancellationToken { get; private set; }

        protected ILockProvider LockProvider { get; }

        protected IRepository Repository { get; }

        protected abstract Task RegisterMessagesAsync();

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            var sopiLifetime = Services.ServiceProvider.GetRequiredService<ISopiApplicationLifetime>();
            CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, sopiLifetime.Stopped).Token;
            return RegisterMessagesAsync();
        }
    }
}