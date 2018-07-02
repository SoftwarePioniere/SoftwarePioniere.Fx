using System;

namespace SoftwarePioniere.DomainModel.Exceptions
{
    /// <summary>
    /// Domänen Logic Fehler
    /// </summary>
    public class DomainLogicException : Exception
    {
        /// <summary>
        /// ctor mit nachrichten übergabe
        /// </summary>
        /// <param name="message"></param>
        public DomainLogicException(string message) : base(message)
        {

        }
    }
}
