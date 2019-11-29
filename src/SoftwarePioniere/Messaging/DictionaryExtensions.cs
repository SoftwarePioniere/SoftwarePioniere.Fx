using System.Collections.Generic;

namespace SoftwarePioniere.Messaging
{
    public static class DictionaryExtensions
    {
        public const string RequestIdKey = "RequestId";

        public const string TraceIdentifierKey = "TraceIdentifier";

        public static Dictionary<string, string> AddProperty(this Dictionary<string, string> dict, string key, string value)
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

        public static string GetProperty(this Dictionary<string, string> dict, string key)
        {
            if (dict == null)
                return null;

            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            return null;
        }

        public static Dictionary<string, string> SetTraceIdentifier(this Dictionary<string, string> dict, string value)
            => dict.AddProperty(TraceIdentifierKey, value);

        public static Dictionary<string, string> SetRequestId(this Dictionary<string, string> dict, string value)
            => dict.AddProperty(RequestIdKey, value);

        public static string GetTraceIdentifier(this Dictionary<string, string> dict) => dict.GetProperty(TraceIdentifierKey);

        public static string GetRequestId(this Dictionary<string, string> dict) => dict.GetProperty(RequestIdKey);
    }
}