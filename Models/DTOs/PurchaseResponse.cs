using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace purchase_service.Models.DTOs;

[ExcludeFromCodeCoverage]
public class PurchaseResponse
{
    [JsonPropertyName("purchase_id")]
    public int PurchaseId { get; set; }
}

