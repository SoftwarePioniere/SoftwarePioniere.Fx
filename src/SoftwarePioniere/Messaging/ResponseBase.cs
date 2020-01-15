using System.Collections.Generic;
using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace SoftwarePioniere.Messaging
{
    public abstract class ResponseBase
    {
        protected ResponseBase()
        {
            Properties = new Dictionary<string, string>();
        }

        [J("is_error")]
        [J1("is_error")]
        public bool IsError => !string.IsNullOrEmpty(Error);

        [J( "error")]
        [J1( "error")]
        public string Error { get; set; }

        [J("properties")]
        [J1("properties")]
        public Dictionary<string, string> Properties { get; set; }

    }

}