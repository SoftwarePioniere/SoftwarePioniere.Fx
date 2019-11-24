using System;
using System.Collections.Generic;

namespace SoftwarePioniere
{
    public static class DictionaryExtensions
    {

        public static void Merge(this IDictionary<string, string> to, IDictionary<string, string> from)
        {
            if (to != null && from != null)
            {
                foreach (var key in from.Keys)
                {
                    if (to.ContainsKey(key))
                    {
                        to.Remove(key);
                    }
                    to.Add(key, from[key]);
                }
            }
        }
        public static void EnsureDictArrayContainsValue(this IDictionary<string, string[]> dict, string key,
            string value)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            dict.TryGetValue(key, out var arrs);
            arrs = arrs.EnsureArrayContainsValue(value);

            if (dict.ContainsKey(key))
            {
                dict[key] = arrs;
            }
            else
            {
                dict.Add(key, arrs);
            }
        }

        public static void EnsureDictArrayNotContainsValue(this IDictionary<string, string[]> dict, string key,
            string value)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            dict.TryGetValue(key, out var arrs);
            arrs = arrs.EnsureArrayNotContainsValue(value);

            if (dict.ContainsKey(key))
            {
                dict[key] = arrs;
            }
            else
            {
                dict.Add(key, arrs);
            }
        }


        public static IDictionary<string, T> EnsureDictContainsValue<T>(this IDictionary<string, T> dict, string key,
            T value)
        {
            if (dict == null)
            {
                dict = new Dictionary<string, T> {{key, value}};
                return dict;
            }

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


        public static IDictionary<string, T> EnsureDictContainsValue<T>(this IDictionary<string, T> dict, string key,
            Action<T> value) where T : class, new()
        {
            var val = new T();
            value?.Invoke(val);

            if (dict == null)
            {
                dict = new Dictionary<string, T> {{key, val}};
                return dict;
            }

            if (dict.ContainsKey(key))
            {
                dict[key] = val;
            }
            else
            {
                dict.Add(key, val);
            }

            return dict;
        }

        public static void EnsureDictNotContainsKey<T>(this IDictionary<string, T> dict, string key)
        {
            if (dict != null)
            {
                if (dict.ContainsKey(key))
                {
                    dict.Remove(key);
                }
            }
        }

        public static void EnsureDictNotContainsKey(this IDictionary<string, string> dict, string key)
        {
            if (dict != null)
            {
                if (dict.ContainsKey(key))
                {
                    dict.Remove(key);
                }
            }
        }

        public static IDictionary<TKey, TValue> Copy<TKey, TValue>(this IDictionary<TKey, TValue> from)
        {
            var dic = new Dictionary<TKey, TValue>();
            if (from != null)
            {
                foreach (var key in from.Keys)
                {
                    dic.Add(key, from[key]);
                }
            }
            return dic;
        }
    }
}