using System;
using System.ComponentModel.DataAnnotations;

using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace SoftwarePioniere.ReadModel
{
    /// <summary>
    /// EntityType pattern for document storing
    /// </summary>
    public abstract class Entity
    {
        protected Entity(string entityType)
        {
            EntityType = entityType;
        }

        ///// <summary>
        ///// id used for MongoDb
        ///// </summary>
        //[MongoDB.Bson.Serialization.Attributes.BsonId]
        //public object BsonId { get; set; }

        /// <summary>
        /// Object unique identifier
        /// </summary>
        [Key]
        [Required]
        [J("id")]
        [J1("id")]
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        public string EntityId { get; set; }

        /// <summary>
        /// Object entityType
        /// </summary>
        [J("entity_type")]
        [J1("entity_type")]
        [Required]
        public string EntityType { get; private set; }

        /// <summary>
        /// Date on last operation
        /// </summary>
        [J("modified_on_utc")]
        [J1("modified_on_utc")]
        [Required]
        public DateTime ModifiedOnUtc { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(EntityId))
                return EntityId;

            return base.ToString();
        }
    }
}
