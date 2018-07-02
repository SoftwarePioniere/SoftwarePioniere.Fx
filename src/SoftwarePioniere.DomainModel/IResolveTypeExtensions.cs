namespace SoftwarePioniere.DomainModel
{
    public static class ResolveTypeExtensions
    {
        public static T Resolve<T>(this IResolveType typeResolver)
        {
            var obj = (T)typeResolver.Resolve(typeof(T));
            return obj;
        }
    }
}
