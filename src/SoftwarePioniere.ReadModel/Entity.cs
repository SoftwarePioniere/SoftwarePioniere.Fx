using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

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
        /// <summary>
        /// Object unique identifier
        /// </summary>
        [Key]
        [Required]
        [JsonProperty("id")]
        public string EntityId { get; set; }

        /// <summary>
        /// Object entityType
        /// </summary>
        [JsonProperty("entity_type")]
        [Required]
        public string EntityType { get; private set; }

        /// <summary>
        /// Date on last operation
        /// </summary>
        [JsonProperty("modified_on_utc")]
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
