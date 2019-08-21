using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.FakeDomain
{

    public class FakeCommandHandler : IHandleMessage<FakeCommand>
    {
        public static IList<Guid> HandledBy { get; } = new List<Guid>();

        public Task HandleAsync(FakeCommand message)
        {
            HandledBy.Add(message.Id);
            return Task.CompletedTask;
        }
    }
}
