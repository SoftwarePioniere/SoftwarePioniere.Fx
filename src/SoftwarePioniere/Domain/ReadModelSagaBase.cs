//using System;
//using Foundatio.Messaging;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using SoftwarePioniere.Messaging;
//using SoftwarePioniere.Messaging.Notifications;
//using SoftwarePioniere.ReadModel;

//// ReSharper disable MemberCanBePrivate.Global

//namespace SoftwarePioniere.DomainModel
//{
//    public abstract class ReadModelSagaBase : SagaBase
//    {
//        protected readonly IEntityStore EntityStore;

//        protected ReadModelSagaBase(ILoggerFactory loggerFactory, IMessageBus bus, IEntityStore entityStore) : base(loggerFactory, bus)
//        {
//            EntityStore = entityStore ?? throw new ArgumentNullException(nameof(entityStore));
//        }

//        // ReSharper disable once MemberCanBeMadeStatic.Global
//        protected virtual NotificationMessage CreateNotification(Entity entity, IMessage msg, string method, object entityToSerialize)
//        {
//            var json = entityToSerialize != null ? JsonConvert.SerializeObject(entityToSerialize) : string.Empty;

//            return new ReadModelUpdatedNotification
//            {
//                EntityId = entity.EntityId,
//                EntityType = entity.EntityType,
//                Method = method,
//                Entity = json,
//                Reason = msg.GetType().FullName
//            }.CreateNotificationMessage(msg);
//        }

//        // ReSharper disable once UnusedMember.Global
//        protected virtual NotificationMessage CreateNotification<T>(EntityDescriptor<T> descriptor, IMessage msg, object entityToSerialize)
//            where T : Entity
//        {
//            var method = ReadModelUpdatedNotification.MethodInsert;

//            if (!descriptor.IsNew)
//                method = ReadModelUpdatedNotification.MethodUpdate;

//            return CreateNotification(descriptor.Entity, msg, method, entityToSerialize);
//        }
//    }
//}
