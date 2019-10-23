using System;
using System.Collections.Concurrent;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Domain
{
    public class EventTypeMap
    {
        private EventTypeMap()
        {
            Mappings.TryAdd("SoftwarePioniere.Messaging.EmptyDomainEvent, SoftwarePioniere.Messaging", typeof(EmptyDomainEvent));
        }

        public static EventTypeMap Instance { get; } = new EventTypeMap();

        public ConcurrentDictionary<string, Type> Mappings { get; } = new ConcurrentDictionary<string, Type>();

        public void Add(string oldType, Type newType)
        {

        }

    }
}
