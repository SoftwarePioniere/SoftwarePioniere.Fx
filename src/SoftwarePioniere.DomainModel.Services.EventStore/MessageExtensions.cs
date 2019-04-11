﻿using System;
using System.Collections.Generic;
using System.Globalization;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using SoftwarePioniere.EventStore;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel
{

    public static class MessageExtensions
    {

        public static EventData ToEventData(this IMessage evnt, IDictionary<string, string> headers)
        {
            var data = JsonConvert.SerializeObject(evnt).ToUtf8();

            if (headers == null)
                headers = new Dictionary<string, string>();

            var type = evnt.GetType();

            var eventHeaders = new Dictionary<string, string>(headers)
            {
              //  {  EventStoreConstants.EventClrTypeHeader, type.AssemblyQualifiedName},
                {  EventStoreConstants.EventTypeHeader, type.GetTypeShortName()},
                {  EventStoreConstants.ServerTimeStampUtcHeader, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)}

            };

            var metadata = JsonConvert.SerializeObject(eventHeaders).ToUtf8();

            var eventName = evnt.GetType().GetEventName();

            return new EventData(evnt.Id, eventName, true, data, metadata);
        }

    }
}