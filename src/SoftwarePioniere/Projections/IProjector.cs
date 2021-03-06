﻿using System.Threading;
using System.Threading.Tasks;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Projections
{
    public interface IProjector
    {

        void Initialize(CancellationToken cancellationToken = default(CancellationToken));

        string StreamName { get; }

        Task ProcessEventAsync(IDomainEvent domainEvent);
    }
}
