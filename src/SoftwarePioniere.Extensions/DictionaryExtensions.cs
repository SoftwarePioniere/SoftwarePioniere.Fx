using System;
using System.Collections.Generic;

namespace SoftwarePioniere
{
    public static class DictionaryExtensions
    {

        public static void Merge(this Dictionary<string, string> to, Dictionary<string, string> from)
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
        public static void EnsureDictArrayContainsValue(this Dictionary<string, string[]> dict, string key,
            string value)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (string.IsNullOrEmpty(key))
                return;

            if (string.IsNullOrEmpty(value))
                return;


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

        public static void EnsureDictArrayNotContainsValue(this Dictionary<string, string[]> dict, string key, string value)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (string.IsNullOrEmpty(key))
                return;

            if (string.IsNullOrEmpty(value))
                return;

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


        public static Dictionary<string, T> EnsureDictContainsValue<T>(this Dictionary<string, T> dict, string key, T value)
        {
            if (dict == null)
            {
                dict = new Dictionary<string, T> { { key, value } };
                return dict;
            }

            if (string.IsNullOrEmpty(key))
                return dict;

            if (value == null)
                return dict;

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


        public static Dictionary<string, T> EnsureDictContainsValue<T>(this Dictionary<string, T> dict, string key, Action<T> value) where T : class, new()
        {
            var val = new T();
            value?.Invoke(val);

            if (dict == null)
            {
                dict = new Dictionary<string, T> { { key, val } };
                return dict;
            }

            if (string.IsNullOrEmpty(key))
                return dict;

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

        public static void EnsureDictNotContainsKey<T>(this Dictionary<string, T> dict, string key)
        {
            if (dict != null && !string.IsNullOrEmpty(key))
            {
                if (dict.ContainsKey(key))
                {
                    dict.Remove(key);
                }
            }
        }

        public static void EnsureDictNotContainsKey(this Dictionary<string, string> dict, string key)
        {
            if (dict != null && !string.IsNullOrEmpty(key))
            {
                if (dict.ContainsKey(key))
                {
                    dict.Remove(key);
                }
            }
        }

        public static Dictionary<TKey, TValue> Copy<TKey, TValue>(this Dictionary<TKey, TValue> from)
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