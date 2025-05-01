namespace Backend.DTO;

public class TransactionStatisticsDTO
{
    public int TotalNumber { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;
}

public class TransactionCreateDto
{
    public Guid UserId { get; set; }
    public Guid MaskId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class TransactionGetDTO
{
    public Guid Id { get; set; }
    public decimal TransactionAmount { get; set; }
    public DateTime TransactionDate { get; set; }

    public required UserBaseDTO User { get; set; }
    public required PharmacyBaseDTO Pharmacy { get; set; }
    public required MaskInfoDTO Mask { get; set; }
}