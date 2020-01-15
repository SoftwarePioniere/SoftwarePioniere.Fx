using System.Collections.Generic;
using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace SoftwarePioniere.ReadModel
{
    /// <summary>
    /// Paged results with continuation token
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResults<T>
    {
        public PagedResults()
        {
            Results = new List<T>();
        }

        /// <summary>
        /// current page
        /// </summary>
        [J("page")]
        [J1("page")]
        public int Page { get; set; }

        /// <summary>
        /// size of the page
        /// </summary>
        [J("page_size")]
        [J1("page_size")]
        public int PageSize { get; set; }


        /// <summary>
        /// results of the current page
        /// </summary>
        [J("result_count")]
        [J1("result_count")]
        public int ResultCount { get; set; }

        /// <summary>
        /// total result count
        /// </summary>
        [J("total_count")]
        [J1("total_count")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Continuation Token for DocumentDB
        /// </summary>
        [J("continuation_token")]
        [J1("continuation_token")]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Results
        /// </summary>
        [J("results")]
        [J1("results")]
        public IList<T> Results { get; set; }

        ///// <summary>
        ///// indicates, that an error occured
        ///// </summary>
        //[J(PropertyName = "is_error")]
        //public bool IsError => !string.IsNullOrEmpty(Error);

        ///// <summary>
        ///// errortext
        ///// </summary>
        //[J(PropertyName = "error")]
        //public string Error { get; set; }

        ///// <summary>
        ///// status code
        ///// </summary>
        //[J(PropertyName = "http_status_code")]
        //public int HttpStatusCode { get; set; } = HttpStatusCode.OK;
    }
}
