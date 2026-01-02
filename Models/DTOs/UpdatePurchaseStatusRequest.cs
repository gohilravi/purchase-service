using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace purchase_service.Models.DTOs;

[ExcludeFromCodeCoverage]
public class UpdatePurchaseStatusRequest
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    [JsonPropertyName("elastic_search_id")]
    public string ElasticSearchId { get; set; }
}

