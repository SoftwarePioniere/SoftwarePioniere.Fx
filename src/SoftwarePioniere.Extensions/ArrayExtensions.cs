using System.Collections.Generic;
using System.Linq;

namespace SoftwarePioniere
{
    public static class ArrayExtensions
    {
        public static T[] GetArray<T>(this T[] arr)
        {
            if (arr == null)
                return new List<T>().ToArray();

            return arr;
        }

        public static T[] EnsureArrayContainsValue<T>(this T[] arr, T val)
        {
            if (arr == null)
                return new[] { val };

            if (!arr.Contains(val))
            {
                var list = new List<T>(arr) { val };
                return list.ToArray();
            }

            return arr;
        }



        public static T[] EnsureArrayNotContainsValue<T>(this T[] arr, T val)
        {
            if (arr != null && arr.Contains(val))
            {
                var list = new List<T>(arr);
                list.Remove(val);
                return list.ToArray();
            }

            return arr;
        }

    }
}
