using Microsoft.AspNetCore.Mvc;

namespace Backend.Parameters;

    public class SearchByNameParameter
    {
        /// <summary>
        /// The type of search (Pharmacy or Mask).
        /// </summary>
        [FromQuery]
        public SearchType Type { get; set; } = SearchType.Pharmacy;

        /// <summary>
        /// The keyword to search for.
        /// </summary>
        [FromQuery]
        public string Keyword { get; set; } = "";

        /// <summary>
        /// The maximum number of results to return.
        /// </summary>
        [FromQuery]
        public int Limit { get; set; } = 10;

        /// <summary>
        /// The number of results to skip.
        /// </summary>
        [FromQuery]
        public int Offset { get; set; } = 0;
    }