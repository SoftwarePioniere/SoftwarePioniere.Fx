using System;
using System.Collections.Concurrent;

namespace SoftwarePioniere.ReadModel
{
    /// <summary>
    /// Speichern der Types, damit man nicht immer eine neue klasse erstellen muss
    /// </summary>
    public class TypeKeyCache
    {
        private readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Den Key zu einem Typen lesen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string GetEntityTypeKey<T>() where T : Entity
        {

            var typeName = typeof(T).FullName;

            if (_cache.TryGetValue(typeName ?? throw new InvalidOperationException(), out var v))
            {
                return v;
            }

            var t = Activator.CreateInstance<T>();

            _cache.TryAdd(typeName, t.EntityType);
            return t.EntityType;

        }

    }
}
