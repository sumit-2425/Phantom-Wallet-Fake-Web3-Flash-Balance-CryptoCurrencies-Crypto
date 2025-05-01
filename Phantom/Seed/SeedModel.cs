using System.Text.Json.Serialization;

namespace Backend.Seeds;

internal class PharmacyJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("cashBalance")]
    public decimal CashBalance { get; set; }
    [JsonPropertyName("openingHours")]
    public string OpeningHours { get; set; } = string.Empty;
    [JsonPropertyName("masks")]
    public List<MaskJson> Masks { get; set; } = [];
}

internal class MaskJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}

internal class UserJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("cashBalance")]
    public decimal CashBalance { get; set; }
    [JsonPropertyName("purchaseHistories")]
    public List<PurchaseHistory> PurchaseHistories { get; set; } = [];
}

internal class PurchaseHistory
{
    [JsonPropertyName("pharmacyName")]
    public string PharmacyName { get; set; } = string.Empty;
    [JsonPropertyName("maskName")]
    public string MaskName { get; set; } = string.Empty;
    [JsonPropertyName("transactionAmount")]
    public decimal TransactionAmount { get; set; }
    [JsonPropertyName("transactionDate")]
    public string TransactionDate { get; set; } = string.Empty;
}