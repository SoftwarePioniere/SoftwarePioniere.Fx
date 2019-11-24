using System.Collections.Generic;

namespace SoftwarePioniere.Messaging
{
    public static class DictionaryExtensions
    {
        public const string RequestIdKey = "RequestId";

        public const string TraceIdentifierKey = "TraceIdentifier";

        public static IDictionary<string, string> AddProperty(this IDictionary<string, string> dict, string key, string value)
        {
            if (dict == null)
                return null;

            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }

            return dict;
        }

        public static string GetProperty(this IDictionary<string, string> dict, string key)
        {
            if (dict == null)
                return null;

            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            return null;
        }

        public static IDictionary<string, string> SetTraceIdentifier(this IDictionary<string, string> dict, string value)
            => dict.AddProperty(TraceIdentifierKey, value);

        public static IDictionary<string, string> SetRequestId(this IDictionary<string, string> dict, string value)
            => dict.AddProperty(RequestIdKey, value);

        public static string GetTraceIdentifier(this IDictionary<string, string> dict) => dict.GetProperty(TraceIdentifierKey);

        public static string GetRequestId(this IDictionary<string, string> dict) => dict.GetProperty(RequestIdKey);
    }
}