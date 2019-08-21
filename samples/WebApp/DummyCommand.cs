﻿using System;
using SoftwarePioniere.Messaging;

namespace WebApp
{
    public class DummyCommand : CommandBase
    {
        public DummyCommand(Guid id, DateTime timeStampUtc, string userId, int originalVersion) 
            : base(id, timeStampUtc, userId, originalVersion, "dummy", id.ToString())
        {
        }
    }
}
