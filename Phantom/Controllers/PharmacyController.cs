using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.DTO;
using Backend.Parameters;

namespace Backend.Controllers
{
    /// <summary>
    /// API relates to search pharmacy info.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PharmacyController : ControllerBase
    {
        private readonly PharmacyService _service;

        public PharmacyController(PharmacyService service)
        {
            _service = service;
        }

        /// <summary>
        /// List all pharmacies open at a specific time and day of week
        /// </summary>
        /// <param name="time">The time for searching open pharmacies in HH:mm format. (in query)</param>
        /// <param name="dayOfWeek">The day of week for searching open pharmacies in abbreviation (ex: Mon). (in query)</param>
        /// <returns>A list of opened pharmacies.</returns>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("opened")]
        public async Task<ActionResult<IEnumerable<OpenedPharmacyDTO>>> GetPharmacies([AsParameters] GetOpenPharmaciesParameter parameter)
        {
            try
            {
                return await _service.GetOpened(parameter);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Gets the list of masks sold in a specific pharmacy.
        /// </summary>
        /// <param name="id">Pharmacy ID (in route)</param>
        /// <param name="sortBy">Masks sort by name or price (in query)</param>
        /// <param name="order">Order in Asc or Desc (in query)</param>
        /// <returns>A list of masks sold in the pharmacy.</returns>
        [HttpGet("{id}/masks")]
        public async Task<ActionResult<IEnumerable<MaskInfoDTO>>> GetPharmacyMasks([FromRoute] Guid id, [AsParameters] GetMasksOfPharmacyParameter parameter)
        {
            try
            {
                return await _service.GetPharmacyMasks(id, parameter);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// List all pharmacies with more or less than x mask products within a price range.
        /// </summary>
        /// <param name="minPrice">Minimum mask price (in query)</param>
        /// <param name="maxPrice">Maximum mask price (in query)</param>
        /// <param name="maskCount">The threshold of mask products (in query)</param>
        /// <param name="isMoreThan">List pharmacy whose mask product amount is more than threshold (in query)</param>
        /// <returns>A list of pharmacies filtered by mask condition.</returns>
        [HttpGet("filter_by_masks")]
        public async Task<ActionResult<IEnumerable<PharmacyFilteredByMaskDTO>>> GetPharmacyMasks([AsParameters] FilterPharmaciesByMaskConditionParameter parameter)
        {
            try
            {
                return await _service.FilterByMaskCondition(parameter);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
