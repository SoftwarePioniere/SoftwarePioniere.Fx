using System;
using System.Collections.Generic;

namespace SoftwarePioniere.ReadModel
{
    public static class CacheKeys 
    {
        public static string Create<T>() where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;
            return prefix;
        }

        public static string Create<T>(IEnumerable<string> values) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object>();
            tmp.AddRange(values);
            return ConcatWithDots(prefix, tmp.ToArray());
        }

        public static string Create<T>(string v1) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1};
            return ConcatWithDots(prefix, tmp.ToArray());
        }

        public static string Create<T>(string v1, int i1, int i2) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1, i1, i2};
            return ConcatWithDots(prefix, tmp.ToArray());
        }

        public static string Create<T>(int i1, int i2, IEnumerable<string> values) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {i1, i2};
            tmp.AddRange(values);
            return ConcatWithDots(prefix, tmp.ToArray());
        }


        public static string Create<T>(string v1, int i1, int i2, IEnumerable<string> values) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1, i1, i2};
            tmp.AddRange(values);
            return ConcatWithDots(prefix, tmp.ToArray());
        }


        public static string Create<T>(string v1, IEnumerable<string> values) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1};
            tmp.AddRange(values);
            return ConcatWithDots(prefix, tmp.ToArray());
        }


        public static string Create<T>(string v1, IEnumerable<string> values, IEnumerable<string> values2)
            where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1};
            tmp.AddRange(values);
            tmp.AddRange(values2);
            return ConcatWithDots(prefix, tmp.ToArray());
        }


        public static string Create<T>(string v1, string v2, IEnumerable<string> values) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1, v2};
            tmp.AddRange(values);
            return ConcatWithDots(prefix, tmp.ToArray());
        }

        public static string Create<T>(string v1, string v2) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1, v2};
            return ConcatWithDots(prefix, tmp.ToArray());
        }

        public static string Create<T>(string v1, string v2, string v3) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1, v2, v3};
            return ConcatWithDots(prefix, tmp.ToArray());
        }

        public static string Create<T>(string v1, string v2, string v3, string v4) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1, v2, v3, v4};
            return ConcatWithDots(prefix, tmp.ToArray());
        }

        public static string Create<T>(string v1, string v2, string v3, IEnumerable<string> values) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1, v2, v3};
            tmp.AddRange(values);
            return ConcatWithDots(prefix, tmp.ToArray());
        }

        public static string Create<T>(string v1, string v2, int i1, int i2) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1, v2, i1, i2};
            return ConcatWithDots(prefix, tmp.ToArray());
        }

        public static string Create<T>(string v1, string v2, int i1, int i2, IEnumerable<string> values)
            where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1, v2, i1, i2};
            tmp.AddRange(values);
            return ConcatWithDots(prefix, tmp.ToArray());
        }


        public static string Create<T>(string v1, string v2, string v3, int i1, int i2) where T : Entity
        {
            var prefix = Activator.CreateInstance<T>().EntityType;

            var tmp = new List<object> {v1, v2, v3, i1, i2};
            return ConcatWithDots(prefix, tmp.ToArray());
        }


//        public static string Create(params object[] values)
//        {
//            return ConcatWithDots("", values);
//        }

        private static string ConcatWithDots(string prefix, params object[] values)
        {
            var tmp = new List<string>();

            if (!string.IsNullOrEmpty(prefix))
            {
                tmp.Add(prefix);
            }

            foreach (var s in values)
                if (s != null && !string.IsNullOrEmpty(s.ToString()))
                {
                    if (tmp.Count > 0)
                    {
                        tmp.Add(":");
                    }

                    tmp.Add(s.ToString());
                }

            return string.Concat(tmp);
        }
    }
}