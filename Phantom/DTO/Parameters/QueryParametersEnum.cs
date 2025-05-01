namespace Backend.Parameters;

    public enum Order
    {
        /// <summary>
        /// Descending order.
        /// </summary>
        DESC,

        /// <summary>
        /// Ascending order.
        /// </summary>
        ASC,
    }

    /// <summary>
    /// Abbreviations for days of the week.
    /// </summary>
    public enum DayOfWeekAbbr
    {
        /// <summary>
        /// Sunday.
        /// </summary>
        Sun = 0,

        /// <summary>
        /// Monday.
        /// </summary>
        Mon = 1,

        /// <summary>
        /// Tuesday.
        /// </summary>
        Tue = 2,

        /// <summary>
        /// Wednesday.
        /// </summary>
        Wed = 3,

        /// <summary>
        /// Thursday.
        /// </summary>
        Thur = 4,

        /// <summary>
        /// Friday.
        /// </summary>
        Fri = 5,

        /// <summary>
        /// Saturday.
        /// </summary>
        Sat = 6,
    }

    /// <summary>
    /// Specifies the criteria for sorting masks.
    /// </summary>
    public enum MaskSortBy
    {
        /// <summary>
        /// Sort by name.
        /// </summary>
        Name,

        /// <summary>
        /// Sort by price.
        /// </summary>
        Price,
    }

    /// <summary>
    /// Specifies the criteria for sorting user transactions.
    /// </summary>
    public enum UserTranscationSortBy
    {
        /// <summary>
        /// Sort by quantity.
        /// </summary>
        Quantity,

        /// <summary>
        /// Sort by amount.
        /// </summary>
        Amount,
    }

    /// <summary>
    /// Specifies the type of search.
    /// </summary>
    public enum SearchType
    {
        /// <summary>
        /// Search for pharmacies.
        /// </summary>
        Pharmacy,

        /// <summary>
        /// Search for masks.
        /// </summary>
        Mask,
    }