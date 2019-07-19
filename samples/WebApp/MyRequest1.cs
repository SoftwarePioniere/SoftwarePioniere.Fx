using System;
using System.Collections.Generic;
using SoftwarePioniere.Messaging;

namespace WebApp
{
    public class MyRequest1 : RequestBase
    {
        public string MyText { get; set; }

        internal FakeCommand CreateFakeCommand(string userId)
        {
            return new FakeCommand(Guid.NewGuid(), TimeStampUtc, userId, -1,
                 Guid.NewGuid().ToString(),
                $"Das ist das Command um {DateTime.Now.Ticks}");

        }

        internal IEnumerable<FakeCommand> CreateFakeCommands(string userId)
        {
            var id = Guid.NewGuid().ToString();
            for (int i = 0; i < 5; i++)
            {
                yield return new FakeCommand(Guid.NewGuid(), TimeStampUtc, userId, -1,
                    id,
                    $"Das ist das Command {i} um {DateTime.Now.Ticks}");
            }

        }
    }


}