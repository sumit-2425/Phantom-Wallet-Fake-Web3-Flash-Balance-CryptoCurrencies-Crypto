using Microsoft.AspNetCore.Mvc;

namespace Backend.Parameters;

/// <summary>
/// Parameters for querying transactions within a date range.
/// </summary>
public class InDateRangeQueryParameter
{
    /// <summary>
    /// The start date for the query.
    /// </summary>
    [FromQuery]
    public DateOnly StartDate { get; set; } = DateOnly.MinValue;

    /// <summary>
    /// The end date for the query.
    /// </summary>
    [FromQuery]
    public DateOnly EndDate { get; set; } = DateOnly.MaxValue;
}

/// <summary>
/// Parameters for querying the top users based on transaction amount within a date range.
/// </summary>
public class TopUsersQueryParameter : InDateRangeQueryParameter
{
    /// <summary>
    /// The maximum number of top users to return.
    /// </summary>
    [FromQuery]
    public int Limit { get; set; } = 10;
}