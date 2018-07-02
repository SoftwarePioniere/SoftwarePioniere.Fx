using System;

namespace SoftwarePioniere.ReadModel
{
    /// <summary>
    /// Fehler, der bei nicht erfolgreicher Autorisierung erfolgt
    /// </summary>
    public class AuthorizationException : Exception
    {
        /// <inheritdoc />
        public AuthorizationException(string message) : base(message)
        {
        }
    }
}
