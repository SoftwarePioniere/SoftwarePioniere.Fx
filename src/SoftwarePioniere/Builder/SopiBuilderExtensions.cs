using System;

namespace SoftwarePioniere.Builder
{
    public static class SopiBuilderExtensions
    {
        public static ISopiBuilder AddFeature<T>(this ISopiBuilder builder, string key, T item)
        {
            //var key = typeof(T).FullName;

            if (!builder.Features.ContainsKey(key))
                builder.Features.Add(key, item);

            return builder;
        }

        public static T GetFeature<T>(this ISopiBuilder builder, string key)
        {
            if (!builder.Features.ContainsKey(key))
                throw  new InvalidOperationException("Feature Key not found");

            return (T) builder.Features[key];
        }
    }
}