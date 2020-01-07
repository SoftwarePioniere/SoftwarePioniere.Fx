using System;
using System.Collections.Generic;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.Messaging;

namespace WebApp
{
    public class MyRequest1 : RequestBase
    {
        public string MyText { get; set; }

        public int MultiCount { get; set; } = 5;

        public int MultiCount2 { get; set; } = 2;

        internal FakeCommand CreateFakeCommand(string userId)
        {
            return new FakeCommand(Guid.NewGuid(), TimeStampUtc, userId, -1,
                 Guid.NewGuid().ToString(),
                $"Das ist das Command um {DateTime.Now.Ticks}");

        }

        internal IEnumerable<FakeCommand> CreateFakeCommands(string userId)
        {
            for (int j = 0; j < MultiCount; j++)
            {
                var id = Guid.NewGuid().ToString();
                for (int i = 0; i < MultiCount2; i++)
                {
                    yield return new FakeCommand(Guid.NewGuid(), TimeStampUtc, userId, -1,
                        id,
                        $"Das ist das Command {i} um {DateTime.Now.Ticks}");
                }

            }

        }
    }


}