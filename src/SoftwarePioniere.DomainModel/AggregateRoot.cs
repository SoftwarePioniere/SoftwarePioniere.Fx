using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SoftwarePioniere.DomainModel.Exceptions;
using SoftwarePioniere.Messaging;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SoftwarePioniere.DomainModel
{
    //public abstract class AggregateRootWithId<TId> : AggregateRoot where TId : IAggregateId
    //{
    //    public TId AggregateId { get; }

    //    public AggregateRootWithId(TId aggregateId)
    //    {
    //        AggregateId = aggregateId;
    //    }
    //}

    /// <summary>
    /// Basis Aggregat Klasse
    /// </summary>
    public abstract class AggregateRoot
    {
        private readonly List<IDomainEvent> _changes = new List<IDomainEvent>();

        /// <summary>
        /// Eindeutige Id des Aggregates
        /// Kann evtl. später durch ein Objekt ersetzt werden
        /// </summary>
        public string Id
        {
            get; private set;
        }

        /// <summary>
        /// Setzt die Id des Aggregats
        /// </summary>
        /// <param name="id"></param>
        public void SetId(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new EmptyAggregateIdException(GetType());

            Id = id;
        }

        /// <summary>
        /// Die aktuelle EventVersion, wird mit jedem ApplyEvent hochgezählt
        /// Ohne Event ist der Wert -1
        /// Das erste Event erzeugt die Version 0
        /// Damit ist es Identisch wie im EventStore
        /// </summary>
        public int Version { get; private set; } = -1;


        /// <summary>
        /// Alle Änderungen, die noch nicht im Event Store gespeichert wurden
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IDomainEvent> GetUncommittedChanges()
        {
            return _changes;
        }

        /// <summary>
        /// Änderungen wurden gespeichert und können hier aus dem Aggregate entfernt werden
        /// </summary>
        public void MarkChangesAsCommitted()
        {
            _changes.Clear();
        }

        /// <summary>
        /// Wendet die Liste der Events auf das Aggregate an.
        /// Diese werden nicht in die Changes Liste aufgenommen
        /// </summary>
        /// <param name="history"></param>
        public void LoadFromHistory(IEnumerable<EventDescriptor> history)
        {
            foreach (var eventDescriptor in history)
            {
                ApplyChange(eventDescriptor.EventData, false);
                Version = eventDescriptor.Version;
            }
        }

        /// <summary>
        /// Ein Event Anwenden und in die Liste der Änderungen aufnehmen
        /// </summary>
        /// <param name="event"></param>
        protected void ApplyChange(IDomainEvent @event)
        {
            ApplyChange(@event, true);
        }

        /// <summary>
        /// Ein Event Anwenden und ggf. in die Liste aufnehmen.
        /// Es wird die Methode passend zum EventType gesucht und dieses ausgeführt
        /// vom Interface IApplyEvent
        /// </summary>
        /// <param name="event"></param>
        /// <param name="isNew"></param>
        // push atomic aggregate changes to local history for further processing (EventStore.SaveEvents)
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