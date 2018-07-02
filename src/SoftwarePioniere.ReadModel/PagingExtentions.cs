using System.Collections.Generic;
using System.Linq;

namespace SoftwarePioniere.ReadModel
{
    public static class PagingExtentions
    {
        /// <summary>
        /// Page a list of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static T[] GetPaged<T>(this IEnumerable<T> items, int pageSize, int page)
        {
            if (items == null)
                return new T[0];

            return pageSize > 0 ? items.Skip((page - 1) * pageSize).Take(pageSize).ToArray() : items.ToArray();
        }

        /// <summary>
        /// create paged results
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static PagedResults<T> GetPagedResults<T>(this IEnumerable<T> items, int pageSize, int page)
        {
            var arr = items as T[] ?? items.ToArray();
            var paged = arr.GetPaged(pageSize, page);

            var pag = new PagedResults<T>
            {
                PageSize = pageSize,
                Page = page,
                ResultCount = paged.Length,
                TotalCount = arr.Length,
                Results = paged
            };

            return pag;

        }
    }
}
