﻿using System;
using Foundatio.Lock;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Domain
{
    public class SagaServices : ISagaServices
    {
        public SagaServices(IMessageBusAdapter bus, IRepository repository, ILockProvider lockProvider, IPersistentSubscriptionFactory persistentSubscriptionFactory, IServiceProvider serviceProvider)
        {
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            LockProvider = lockProvider ?? throw new ArgumentNullException(nameof(lockProvider));
            PersistentSubscriptionFactory = persistentSubscriptionFactory ?? throw new ArgumentNullException(nameof(persistentSubscriptionFactory));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IMessageBusAdapter Bus { get; }
        public IRepository Repository { get; }
        public ILockProvider LockProvider { get; }
        public IPersistentSubscriptionFactory PersistentSubscriptionFactory { get; }
        public IServiceProvider ServiceProvider { get; }
    }
}