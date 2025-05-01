using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Parameters;
using Backend.DTO;

namespace Backend.Controllers
{
    /// <summary>
    /// API for searching
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _service;

        public SearchController(SearchService service)
        {
            _service = service;
        }

        /// <summary>
        /// Searches for masks or pharmacies by name keyword.
        /// </summary>
        /// <param name="type">The type (Pharamcy or Mask) of searching. (in query)</param>
        /// <param name="keyword">The keyword to search. (in query)</param>
        /// <param name="limit">The number of results returned. (in query)</param>
        /// <param name="offset">The number of first results to skip. (in query)</param>
        /// <returns>The search result containing masks that match the search criteria.</returns>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        public async Task<ActionResult<SearchResultDTO>> Search([AsParameters] SearchByNameParameter parameter)
        {
            try
            {
                return await _service.Search(parameter);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

    }
}
