using Backend.Interface;

namespace Backend.DTO;

public class PharmacyBaseDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}
public class OpenedPharmacyDTO : PharmacyBaseDTO
{
    public int Week { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
}

public class PharmacyFilteredByMaskDTO : PharmacyBaseDTO
{
    public int NumberOfMasks { get; set; } = 0;
}