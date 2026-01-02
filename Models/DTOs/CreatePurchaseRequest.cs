using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace purchase_service.Models.DTOs;

[ExcludeFromCodeCoverage]
public class CreatePurchaseRequest
{
    [JsonPropertyName("offer_id")]
    public int OfferId { get; set; }
    
    [JsonPropertyName("buyer_id")]
    public int BuyerId { get; set; }
}

