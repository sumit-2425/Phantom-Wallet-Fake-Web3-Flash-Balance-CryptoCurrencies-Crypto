namespace Backend.Models;

/// <summary>
/// Represents a type of mask.
/// </summary>
public class MaskType : BaseEntity
{
    /// <summary>
    /// The name of the mask type.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The color of the mask type.
    /// </summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// The quantity per pack of masks.
    /// </summary>
    public int Quantity { get; set; } = 0;

    /// <summary>
    /// The list of masks associated with this mask type.
    /// </summary>
    public List<Mask> Masks { get; set; } = [];

}