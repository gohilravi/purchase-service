using System.Text.Json.Serialization;

namespace purchase_service.Models.DTOs;

public class CreatePurchaseRequest
{
    [JsonPropertyName("offer_id")]
    public int OfferId { get; set; }
    
    [JsonPropertyName("buyer_id")]
    public int BuyerId { get; set; }
}

