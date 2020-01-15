using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace SoftwarePioniere.ReadModel
{
    /// <summary>
    /// Infos zum EntityType
    /// </summary>
    public class EntityTypeInfo
    {
        /// <summary>
        /// Der Klassen Name
        /// </summary>
        [J("name")]
        [J1("name")]
        public string Name { get; set; }

        /// <summary>
        /// Der vollständige Klassen Name
        /// </summary>
        [J("full_name")]
        [J1("full_name")]
        public string FullName { get; set; }

        /// <summary>
        /// EntityType der Klasse
        /// </summary>
        [J("type_key")]
        [J1("type_key")]
        public string TypeKey { get; set; }
    }
}
