using System;
using System.Collections.Generic;

namespace SoftwarePioniere.ReadModel
{
    /// <summary>
    /// Speichern der Types, damit man nicht immer eine neue klasse erstellen muss
    /// </summary>
    public class TypeKeyCache
    {
        private readonly object _thisLock = new object();
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
        
        /// <summary>
        /// Den Key zu einem Typen lesen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string GetEntityTypeKey<T>() where T : Entity
        {
            lock (_thisLock)
            {
                var typeName = typeof(T).FullName;

                if (_cache.ContainsKey(typeName ?? throw new InvalidOperationException()))
                    return _cache[typeName];

                var t = Activator.CreateInstance<T>();

                if (!_cache.ContainsKey(typeName))
                    _cache.Add(typeName, t.EntityType);
                return t.EntityType;
            }

        }

    }
}
