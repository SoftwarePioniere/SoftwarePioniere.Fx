using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftwarePioniere.ReadModel
{
    public static class EntityStoreExtensions
    {
        public static async Task<EntityDescriptor<T>> LoadAsync<T>(this IEntityStore store, string itemOnlyId, CancellationToken cancellationToken = default(CancellationToken)) where T : Entity
        {
            var id = itemOnlyId.CalculateEntityId<T>();

            var desc = new EntityDescriptor<T>
            {
                Id = itemOnlyId,
                EntityId = id
            };

            var o = await store.LoadItemAsync<T>(id, cancellationToken);

            if (o == null)
            {
                desc.IsNew = true;
                o = Activator.CreateInstance<T>();
                o.SetEntityId(itemOnlyId);
            }
            desc.Entity = o;

            return desc;
        }

        public static Task SaveAsync<T>(this IEntityStore store, EntityDescriptor<T> ent, CancellationToken cancellationToken = default(CancellationToken)) where T : Entity
        {

            if (ent.IsNew)
                return store.InsertItemAsync(ent.Entity, cancellationToken);

            return store.UpdateItemAsync(ent.Entity, cancellationToken);
        }

        public static async Task<T> LoadEntity<T>(this IEntityStore store, string entityIdValue, CancellationToken cancellationToken = default(CancellationToken)) where T : Entity
        {
            var ent = await store.LoadItemAsync<T>(entityIdValue.CalculateEntityId<T>(), cancellationToken);
            return ent;
        }
    }
}