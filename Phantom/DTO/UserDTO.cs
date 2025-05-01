namespace Backend.DTO;

public class UserBaseDTO {
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}

public class UserWithTotalTranscationAmountDTO : UserBaseDTO {
    public decimal TotalTranscationAmount { get; set; } = 0;
}