namespace Backend.Models;

/// <summary>
/// Base class with Id, CreateAt, UpdateAt
/// </summary>
public class BaseEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// Record Create Time
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Record Create Time
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}