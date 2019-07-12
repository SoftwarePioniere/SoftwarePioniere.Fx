using System;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Lock;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Domain
{
    public abstract class SagaBase2 : ISaga
    {
        protected readonly IMessageBusAdapter Bus;

        protected readonly ILogger Logger;

        protected SagaBase2(ILoggerFactory loggerFactory, ISagaServices services)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            Logger = loggerFactory.CreateLogger(GetType());
            Bus = services.Bus;
            Repository = services.Repository;
            LockProvider = services.LockProvider;
        }

        protected CancellationToken CancellationToken { get; private set; }

        protected ILockProvider LockProvider { get; }

        protected IRepository Repository { get; }

        protected abstract Task RegisterMessagesAsync();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            return RegisterMessagesAsync();
        }
    }
}