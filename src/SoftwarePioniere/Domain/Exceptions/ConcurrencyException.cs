using System;

namespace SoftwarePioniere.Domain.Exceptions
{
    /// <summary>
    /// Wird geworfen, wenn eine inkonsistenz auftritt consistenz 
    /// </summary>
    public class ConcurrencyException : SopiException
    {
        public ConcurrencyException()
        {
        }

        public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Tatsächliche Event EventVersion
        /// </summary>
        public int CurrentVersion { get; set; }
        /// <summary>
        /// Event EventVersion, die erwartet wurde
        /// </summary>
        public int ExpectedVersion { get; set; }
        /// <summary>
        /// Typ des Aggregats
        /// </summary>
        public Type AggregateType { get; set; }
    }
}