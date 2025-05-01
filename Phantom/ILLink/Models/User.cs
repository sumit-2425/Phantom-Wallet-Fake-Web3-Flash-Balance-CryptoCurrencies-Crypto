using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// Represents a user.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// The name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The cash balance of the user.
    /// </summary>
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CashBalance { get; set; }

    /// <summary>
    /// The list of transactions associated with the user.
    /// </summary>
    public List<Transaction> Transactions { get; set; } = [];
}