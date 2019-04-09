using System;

namespace SoftwarePioniere
{
    public static class EnumExtensions
    {

        public static T ParseEnum<T>(this string s)
        {
            return (T)Enum.Parse(typeof(T), s);
        }

        //public static TOut MapByStringValue<TIn, TOut>(TIn x, bool ignoreCase = false)
        //    where TOut : struct
        //{
        //    var stringValue = x.ToString();
        //    var y = (TOut)Enum.Parse(typeof(TOut), stringValue, ignoreCase);

        //    return y;
        //}
    }
}