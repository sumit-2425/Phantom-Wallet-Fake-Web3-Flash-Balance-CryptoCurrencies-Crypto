using Backend.Interface;
using Backend.Models;

namespace Backend.DTO;

public class MaskBaseDTO
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
}
public class MaskInfoDTO : MaskBaseDTO
{
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
    public int QuantityPerPack { get; set; } = 0;
}