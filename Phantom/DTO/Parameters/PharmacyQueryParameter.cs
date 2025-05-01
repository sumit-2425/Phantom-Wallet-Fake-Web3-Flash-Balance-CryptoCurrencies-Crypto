using Microsoft.AspNetCore.Mvc;

namespace Backend.Parameters;

    public class GetOpenPharmaciesParameter
    {
        /// <summary>
        /// The time to search for opening pharmacies.
        /// </summary>
        [FromQuery]
        public TimeOnly Time { get; set; }

        /// <summary>
        /// The day of the week to search for open pharmacies.
        /// </summary>
        [FromQuery]
        public DayOfWeekAbbr DayOfWeek { get; set; }
    }

    /// <summary>
    /// Parameters for getting masks of a specific pharmacy.
    /// </summary>
    public class GetMasksOfPharmacyParameter
    {
        /// <summary>
        /// The criteria for sorting masks.
        /// </summary>
        [FromQuery]
        public MaskSortBy SortBy { get; set; } = MaskSortBy.Name;

        /// <summary>
        /// The order for sorting masks.
        /// </summary>
        [FromQuery]
        public Order Order { get; set; } = Order.ASC;
    }

    /// <summary>
    /// Parameters for filtering pharmacies by mask condition.
    /// </summary>
    public class FilterPharmaciesByMaskConditionParameter
    {
        /// <summary>
        /// The minimum price of masks.
        /// </summary>
        [FromQuery]
        public decimal MinPrice { get; set; } = 0;

        /// <summary>
        /// The maximum price of masks.
        /// </summary>
        [FromQuery]
        public decimal MaxPrice { get; set; } = decimal.MaxValue;

        /// <summary>
        /// The mask count for filtering.
        /// </summary>
        [FromQuery]
        public int MaskCount { get; set; } = 0;

        /// <summary>
        /// Whether to filter pharmacies with more than the specified mask count.
        /// </summary>
        [FromQuery]
        public bool IsMoreThan { get; set; } = true;
    }