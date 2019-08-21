using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftwarePioniere.ReadModel
{
    public static class EntityExtensions
    {
        public static void SetEntityId(this Entity item, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            item.EntityId = CalculateEntityId(item, value);
        }

        public static string CalculateEntityId(this Entity item, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return string.Concat(item.EntityType, ":", value);
        }

        public static string CalculateEntityId<T>(this string value) where T : Entity
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var ent = Activator.CreateInstance<T>();

            return ent.CalculateEntityId(value);
        }

          public static string ExtractIdValue<T>(this string entityId) where T: Entity
        {
            var sendung = Activator.CreateInstance<T>();
            var id = entityId.Replace($"{sendung.EntityType}:", string.Empty);
            return id;
        }
        
        public static T[] DistinctArray<T>(this IEnumerable<T> items) where T : Entity
        {
            if (items == null)
                return new T[0];

            return items.GroupBy(x => x.EntityId).Select(g => g.First()).ToArray();
        }


    }
}
