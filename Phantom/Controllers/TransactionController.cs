using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Parameters;
using Backend.DTO;
using Backend.Exceptions;

namespace Backend.Controllers
{
    /// <summary>
    /// API for transaction-related.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _service;

        public TransactionController(TransactionService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets the top users based on total transaction amount within a date range..
        /// </summary>
        /// <param name="startDate">The start date of date range. (in query)</param>
        /// <param name="endDat">The end date of date range. (in query)</param>
        /// <param name="limit">The number of results returned.</param>
        /// <returns>A list of users with their total transaction amounts.</returns>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("top_users")]
        public async Task<ActionResult<IEnumerable<UserWithTotalTranscationAmountDTO>>> GetTopUsers([FromQuery] TopUsersQueryParameter parameter)
        {
            try
            {
                return await _service.GetTopUser(parameter);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Gets transaction statistics within a date range.
        /// </summary>
        /// <param name="startDate">The start date of date range. (in query)</param>
        /// <param name="endDat">The end date of date range. (in query)</param>
        /// <returns>The transaction statistics within the specified date range.</returns>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("statistics")]
        public async Task<ActionResult<TransactionStatisticsDTO>> GetStatistics([AsParameters] InDateRangeQueryParameter parameter)
        {
            try
            {
                return await _service.GetStatistics(parameter);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        /// <param name="userId">The user Id of buying mask. (in body)</param>
        /// <param name="maskId">The Id of mask sold in the transaction. (in body)</param>
        /// <param name="quantity">The amount of mask sold. (in body)</param>
        /// <returns>The created transaction.</returns>
        /// <response code="422">Transcation validation failed</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        public async Task<ActionResult<TransactionGetDTO>> CreateTransaction([FromBody] TransactionCreateDto body)
        {
            try
            {
                var transaction = await _service.Create(body);
                return CreatedAtAction("GetTransaction", new { id = transaction.Id }, transaction);
            }
            catch (TransactionCreateValidationException ex)
            {
                return UnprocessableEntity(ex.Errors[0]);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Gets a specific transaction by its Id.
        /// </summary>
        /// <param name="id">The id of the transaction.</param>
        /// <returns>The transaction details.</returns>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionGetDTO>> GetTransaction([FromRoute] Guid id)
        {
            try
            {
                var transaction = await _service.GetTransaction(id);

                if (transaction == null) return NotFound();

                return transaction;
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
