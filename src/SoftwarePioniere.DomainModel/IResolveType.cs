using System;

namespace SoftwarePioniere.DomainModel
{
    /// <summary>
    /// Dient zum erzeugen der Objekt Instanzen
    /// facade für DI
    /// </summary>
    public interface IResolveType
    {
        /// <summary>
        /// Objekt eines Typs erzeugen
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        object Resolve(Type t);
    }
}
