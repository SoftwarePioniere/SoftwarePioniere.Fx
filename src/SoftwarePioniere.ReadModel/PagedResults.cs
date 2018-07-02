using System.Collections.Generic;
using Newtonsoft.Json;

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
        [JsonProperty("page")]
        public int Page { get; set; }

        /// <summary>
        /// size of the page
        /// </summary>
        [JsonProperty("page_size")]
        public int PageSize { get; set; }


        /// <summary>
        /// results of the current page
        /// </summary>
        [JsonProperty("result_count")]
        public int ResultCount { get; set; }

        /// <summary>
        /// total result count
        /// </summary>
        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Continuation Token for DocumentDB
        /// </summary>
        [JsonProperty("continuation_token")]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Results
        /// </summary>
        [JsonProperty("results")]
        public IList<T> Results { get; set; }

        ///// <summary>
        ///// indicates, that an error occured
        ///// </summary>
        //[JsonProperty(PropertyName = "is_error")]
        //public bool IsError => !string.IsNullOrEmpty(Error);

        ///// <summary>
        ///// errortext
        ///// </summary>
        //[JsonProperty(PropertyName = "error")]
        //public string Error { get; set; }

        ///// <summary>
        ///// status code
        ///// </summary>
        //[JsonProperty(PropertyName = "http_status_code")]
        //public int HttpStatusCode { get; set; } = HttpStatusCode.OK;
    }
}
