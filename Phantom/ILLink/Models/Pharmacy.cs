using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// Represents a pharmacy.
/// </summary>
public class Pharmacy : BaseEntity
{
    /// <summary>
    /// The name of the pharmacy.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The cash balance of the pharmacy.
    /// </summary>
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CashBalance { get; set; }

    /// <summary>
    /// The list of masks sold in the pharmacy.
    /// </summary>
    public List<Mask> Masks { get; set; } = [];

    /// <summary>
    /// The list of openingHours of the pharmacy.
    /// </summary>
    public List<OpeningHour> OpeningHours { get; set; } = [];
    public List<Transaction> Transactions { get; set; } = [];
}