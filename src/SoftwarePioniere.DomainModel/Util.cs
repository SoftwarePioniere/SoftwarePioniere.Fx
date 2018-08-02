using System;
using System.Reflection;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel
{
    /// <summary>
    /// Diverse Utils
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// ermittelt aus einem typen und er id den stream namen
        /// in einem Stream werden alle Events eines Aggregats zusammen gefasstt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string AggregateIdToStreamName<T>(string id)
        {
            var type = typeof(T);
            return AggregateIdToStreamName(type, id);
        }

        public static string GetAggregateName(this Type type)
        {
            var aggNameAttr = type.GetTypeInfo().GetCustomAttribute<AggregateNameAttribute>();
            if (aggNameAttr != null)
            {
                return aggNameAttr.AggregateStreamName;
            }

            return type.Name;
        }

        public static string AggregateIdToStreamName(Type type, string id)
        {
            var aggName = GetAggregateName(type);
            return string.Format("{0}-{1}", aggName, id.ToUpper().Replace("-", ""));
        }

        //public static DomainEventMessage CreateDomainEventMessage(this IDomainEvent @event, string eventType, string aggreateName, string aggregateId) 
        //{
        //    var ev = new DomainEventMessage(Guid.NewGuid(), @event.TimeStampUtc, @event.UserId,
        //        aggreateName, aggregateId,  eventType, @event);

        //    return ev;
        //}

        public static DomainEventMessage CreateDomainEventMessage<TDomainEvent>(this TDomainEvent @event, string aggreateName, string aggregateId) where TDomainEvent : IDomainEvent
        {

            var ev = new DomainEventMessage(Guid.NewGuid(), @event.TimeStampUtc, @event.UserId,
                aggreateName, aggregateId, typeof(TDomainEvent).GetTypeShortName(), @event);

            return ev;
        }

        public static DomainEventMessage CreateDomainEventMessageFromType(this IDomainEvent @event, string aggreateName, string aggregateId, Type eventType)
        {

            //    var t = typeof(DomainEventMessage<>).MakeGenericType(@event.GetType());

            //var ev = Activator.CreateInstance(t, new object[] {
            //    Guid.NewGuid(), @event.TimeStampUtc, @event.UserId,
            //    // ReSharper disable once UsePatternMatching
            //    aggreateName, aggregateId,  @event.GetType().Name, @event}) as IMessage;

            var ev = new DomainEventMessage(Guid.NewGuid(), @event.TimeStampUtc, @event.UserId,
                aggreateName, aggregateId, eventType.GetTypeShortName(), @event);

            if (ev == null)
                throw new InvalidOperationException("cannot create DomainEventMessage");

            return ev;
        }

        public static T Cast<T>(this DomainEventMessage domainEventMessage) where T : IDomainEvent
        {
            return (T) domainEventMessage.DomainEvent;
        }

    }
}
