using System;

namespace SoftwarePioniere.DomainModel.Exceptions
{
    /// <summary>
    /// Wird geworfen, wenn ein aggregate gelöscht wurde 
    /// </summary>
    public class AggregateDeletedException : Exception
    {
        /// <summary>
        /// Die id des Aggregats
        /// </summary>
        public string AggregateId { get; set; }

        /// <summary>
        /// Typ des Aggregats
        /// </summary>

        public Type AggregateType { get; set; }
    }
}