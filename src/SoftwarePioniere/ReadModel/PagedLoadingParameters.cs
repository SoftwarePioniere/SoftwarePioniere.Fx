using System;
using System.Linq.Expressions;

namespace SoftwarePioniere.ReadModel
{
    public class PagedLoadingParameters<T>
    {
        public int PageSize { get; set; } = -1;

        public string ContinuationToken { get; set; }

        public int Page { get; set; }

        public Expression<Func<T,bool>> Where { get; set; }

        public Expression<Func<T,object>> OrderByDescending { get; set; }

        public Expression<Func<T, object>> OrderBy { get; set; }

        //public Expression<Func<T, object>> OrderThenBy { get; set; }
    }
}