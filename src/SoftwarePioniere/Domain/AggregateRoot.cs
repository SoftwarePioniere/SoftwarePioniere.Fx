using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SoftwarePioniere.Domain.Exceptions;
using SoftwarePioniere.Messaging;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SoftwarePioniere.Domain
{
    //public abstract class AggregateRootWithId<TId> : AggregateRoot where TId : IAggregateId
    //{
    //    public TId AggregateId { get; }

    //    public AggregateRootWithId(TId aggregateId)
    //    {
    //        AggregateId = aggregateId;
    //    }
    //}


    public abstract class AggregateRoot : IAggregateRoot
    {
        private readonly List<IDomainEvent> _changes = new List<IDomainEvent>();


        public string Id
        {
            get; private set;
        }

        public void SetId(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new EmptyAggregateIdException(GetType());

            Id = id;
        }

        public static int StartVersion = -1;

        /// <summary>
        /// Die aktuelle EventVersion, wird mit jedem ApplyEvent hochgezählt
        /// Ohne Event ist der Wert -1
        /// Das erste Event erzeugt die Version 0
        /// Damit ist es Identisch wie im EventStore
        /// </summary>
        public int Version { get; private set; } = StartVersion;


        public IEnumerable<IDomainEvent> GetUncommittedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsCommitted()
        {
            _changes.Clear();
        }

        public void LoadFromHistory(IEnumerable<EventDescriptor> history)
        {
            foreach (var eventDescriptor in history)
            {
                ApplyChange(eventDescriptor.EventData, false);
                Version = eventDescriptor.Version;
            }
        }

        protected void ApplyChange(IDomainEvent @event)
        {
            ApplyChange(@event, true);
        }

        private void ApplyChange(IDomainEvent @event, bool isNew)
        {
            //den IApplyEvent mit dem generischen Typ des DomainEvents beziehen
            var ixtype = typeof(IApplyEvent<>).MakeGenericType(@event.GetType());

            //die erste methode des interfaces beziehen, diese wird ausgeführt
            var ixmethod = ixtype.GetTypeInfo().GetMethods().FirstOrDefault();

            if (ixmethod != null)
            {
                var thisImplementsType = GetType().GetTypeInfo().ImplementedInterfaces.Contains(ixtype);
                if (thisImplementsType)
                {
                    //die methode aus dem interface auf dem aktuellen objekt anwenden
                    ixmethod.Invoke(this, new object[] { @event });
                }
            }

            if (isNew)
            {
                _changes.Add(@event);
                Version++;
            }
        }

    }
}