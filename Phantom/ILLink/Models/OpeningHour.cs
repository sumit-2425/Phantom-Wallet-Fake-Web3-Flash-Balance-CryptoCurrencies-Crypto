using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// Represents the opening hours of a pharmacy.
/// </summary>
public class OpeningHour : BaseEntity
{
    /// <summary>
    /// The day of the week (0-6, where 0 is Sunday and 6 is Saturday).
    /// </summary>
    [Required]
    [Range(0, 6)]
    public ushort Week { get; set; }

    /// <summary>
    /// The opening time of the pharmacy.
    /// </summary>
    [Required]
    [Column(TypeName = "time")]
    [DataType(DataType.Time)]
    public TimeOnly OpenTime { get; set; }

    /// <summary>
    /// The closing time of the pharmacy.
    /// </summary>
    [Required]
    [Column(TypeName = "time")]
    [DataType(DataType.Time)]
    public TimeOnly CloseTime { get; set; }

    /// <summary>
    /// The pharmacy associated with this opening hours.
    /// </summary>
    [Required]
    [ForeignKey("Pharmacy")]
    public Guid PharmacyId { get; set; }
    public Pharmacy Pharmacy { get; set; } = null!;
}