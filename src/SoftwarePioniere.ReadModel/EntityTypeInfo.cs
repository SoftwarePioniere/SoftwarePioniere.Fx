using Newtonsoft.Json;

namespace SoftwarePioniere.ReadModel
{
    /// <summary>
    /// Infos zum EntityType
    /// </summary>
    public  class EntityTypeInfo
    {
        /// <summary>
        /// Der Klassen Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Der vollständige Klassen Name
        /// </summary>
        [JsonProperty("full_name")]
        public string FullName { get; set; }

        /// <summary>
        /// EntityType der Klasse
        /// </summary>
        [JsonProperty("type_key")]
        public string TypeKey { get; set; }
    }
}
